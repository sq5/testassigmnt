// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ARCHIVE.COMMON.DTOModels.UI;
using ARCHIVE.COMMON.Entities;
using Microsoft.AspNetCore.Routing;
using ARCHIVE.COMMON.DTOModels.Admin;
using ARCHIVE.COMMON.DTOModels;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Collections.Generic;
using COMMON.Utilities;
using COMMON.Common.Services.ContextService;

namespace API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MetadataController : Controller
    {
        private readonly IContextService _context;
        private readonly string[] SourceArray = { "Входящий", "Исходящий" };

        public MetadataController(IContextService contextService)
        {
            _context = contextService;
        }

        [HttpPost]
        public async Task<ActionResult<MetadataDTO>> AddOrUpdateNonFormDoc(NonFormDocsDTO model)
        {
            string methodName = System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.Name;
            try
            {
                if (model == null)
                {
                    await _context.CommonService.CreateLog("",
                        0,
                        "",
                        methodName,
                        StatusCodes.Status400BadRequest,
                       "No entity provider");
                    return BadRequest("No entity provider");
                }
                if (model.Binaries == null || model.Binaries.Count == 0)
                {
                    await _context.CommonService.CreateLog(model.RequestID,
                        model.ClientId,
                        Newtonsoft.Json.JsonConvert.SerializeObject(model),
                        methodName,
                        StatusCodes.Status400BadRequest,
                       "File is required");
                    return BadRequest("File is required");
                }

                foreach (BinariesDTO binFile in model.Binaries)
                {
                    if (string.IsNullOrEmpty(binFile.FileBase64))
                    {
                        await _context.CommonService.CreateLog(model.RequestID,
                            model.ClientId,
                            Newtonsoft.Json.JsonConvert.SerializeObject(model),
                            methodName,
                            StatusCodes.Status400BadRequest,
                           "No binaries FileBase64 string");
                        return BadRequest("No binaries FileBase64 string");
                    }
                    if (string.IsNullOrEmpty(binFile.FileName))
                    {
                        await _context.CommonService.CreateLog(model.RequestID,
                            model.ClientId,
                            Newtonsoft.Json.JsonConvert.SerializeObject(model),
                            methodName,
                            StatusCodes.Status400BadRequest,
                           "No file name");
                        return BadRequest("No file name");
                    }
                    try
                    {
                        var res = _context.CommonService.GetOriginalLengthInMB(binFile.FileBase64);
                        if (res > 20)
                        {
                            await _context.CommonService.CreateLog(model.RequestID,
                                model.ClientId,
                                Newtonsoft.Json.JsonConvert.SerializeObject(model),
                                methodName,
                                StatusCodes.Status400BadRequest,
                               "Max File size exceeded ( #err-20)");
                            return BadRequest("Max File size exceeded ( #err-20)");
                        }
                    }
                    catch
                    {
                        await _context.CommonService.CreateLog(model.RequestID,
                            model.ClientId,
                            Newtonsoft.Json.JsonConvert.SerializeObject(model),
                            methodName,
                            StatusCodes.Status400BadRequest,
                           "Can not parse File to Base64");
                        return BadRequest("Can not parse File to Base64");
                    }
                }

                var checkAuth = await CheckAuthorization();
                if ((checkAuth.res as ObjectResult).StatusCode != 200)
                    return checkAuth.res;
                model.ClientId = checkAuth.clientId;
                if (string.IsNullOrEmpty(model.RequestID))
                {
                    await _context.CommonService.CreateLog(model.RequestID,
                        model.ClientId,
                        Newtonsoft.Json.JsonConvert.SerializeObject(model),
                        methodName,
                        StatusCodes.Status400BadRequest,
                       "Field RequestID is required");
                    return BadRequest("Field RequestID is required");
                }
                model.Created = DateTime.Now;
                model.Modified = DateTime.Now;
                var idDoc = await _context.DataBase.CreateAsyncInt32<NonFormDocsDTO, NonFormDocs>(model);
                var dto = Ensol.CommonUtils.Common.GetNonFormDocByID(idDoc, _context.DbContext, checkAuth.clientId);
                if (dto == null)
                {
                    await _context.CommonService.CreateLog(model.RequestID,
                        model.ClientId,
                        Newtonsoft.Json.JsonConvert.SerializeObject(model),
                        methodName,
                        StatusCodes.Status400BadRequest,
                       "Unable to add the entity");
                    return BadRequest("Unable to add the entity");
                }

                BinariesDTO bFile = model.Binaries.FirstOrDefault();

                var file = await _context.DataBase.SingleAsync<DocFile, BinariesDTO>(f => f.NonFormDocId.Equals(dto.Id) && f.FileName.Equals(bFile.FileName));
                await _context.CommonService.CreateOrUpdateFile(file, bFile, dto.Id, dto.ClientId, "nonform");

                await _context.CommonService.CreateLog(model.RequestID,
                                model.ClientId,
                                Newtonsoft.Json.JsonConvert.SerializeObject(model),
                                methodName,
                                StatusCodes.Status201Created);
                var uri = _context.LinkGenerator.GetPathByAction("AddOrUpdateNonFormDoc", "Metadata");
                return Created(uri, dto);
            }
            catch (Exception ex)
            {
                await _context.CommonService.CreateLog(model.RequestID,
                                model.ClientId,
                                Newtonsoft.Json.JsonConvert.SerializeObject(model),
                                methodName,
                                StatusCodes.Status500InternalServerError,
                                "Ошибка: " + ex.Message + " StackException: " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpPost]
        public async Task<ActionResult> DocApproval(DocApprovalDTO model)
        {
            string methodName = System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.Name;
            int clientId = -1;
            try
            {
                if (model == null)
                {
                    await _context.CommonService.CreateLog(model.RequestID,
                        0,
                        "",
                        methodName,
                        StatusCodes.Status400BadRequest,
                       "No entity provider");
                    return BadRequest("No entity provider");
                }
                var checkAuth = await CheckAuthorization();
                if ((checkAuth.res as ObjectResult).StatusCode != 200)
                    return checkAuth.res;
                clientId = checkAuth.clientId;
                if (string.IsNullOrEmpty(model.RequestID))
                {
                    await _context.CommonService.CreateLog(model.RequestID,
                        clientId,
                        Newtonsoft.Json.JsonConvert.SerializeObject(model),
                        methodName,
                        StatusCodes.Status400BadRequest,
                       "Field RequestID is required");
                    return BadRequest("Field RequestID is required");
                }
                if (model.NonFormID == 0)
                {
                    await _context.CommonService.CreateLog(model.RequestID,
                        clientId,
                        Newtonsoft.Json.JsonConvert.SerializeObject(model),
                        methodName,
                        StatusCodes.Status400BadRequest,
                       "Field NonFormID is required");
                    return BadRequest("Field NonFormID is required");
                }

                NonFormDocsDTO nonFormDoc = null;

                nonFormDoc = Ensol.CommonUtils.Common.GetNonFormDocByID(model.NonFormID, _context.DbContext, clientId);
                if (nonFormDoc == null)
                {
                    await _context.CommonService.CreateLog(model.RequestID,
                        clientId,
                        Newtonsoft.Json.JsonConvert.SerializeObject(model),
                        methodName,
                        StatusCodes.Status400BadRequest,
                       "No NonFormDoc with id " + model.NonFormID + " was found");
                    return BadRequest("No NonFormDoc with id " + model.NonFormID + " was found");
                }


                if (model.Approved == true)
                {
                    await _context.CommonService.CreateLog(model.RequestID,
                        clientId,
                        Newtonsoft.Json.JsonConvert.SerializeObject(model),
                        methodName,
                        StatusCodes.Status400BadRequest,
                       "Wrong approval for document was found");
                    return BadRequest("Wrong approval for document was found");
                }

                nonFormDoc.Declined = true;
                nonFormDoc.Deleted = true;
                nonFormDoc.DeleteDate = DateTime.Now;
                if (!string.IsNullOrEmpty(model.Approver))
                    nonFormDoc.DeclinedBy = model.Approver;
                if (!string.IsNullOrEmpty(model.Comment))
                    nonFormDoc.Comment = model.Comment;
                nonFormDoc.Modified = DateTime.Now;

                await _context.DataBase.UpdateAsync<NonFormDocsDTO, NonFormDocs>(nonFormDoc);

                if (!string.IsNullOrEmpty(nonFormDoc.Sender))
                {
                    var file = await _context.DataBase.SingleAsync<DocFile, DocFile>(f => f.NonFormDocId.Equals(nonFormDoc.Id));
                    string EventText = string.Format("Ваш документ {0} был отклонен в 1С", "link", nonFormDoc.FileName);
                    await _context.CommonService.CreateUserEvent(nonFormDoc.Sender, EventText, DateTime.Now, 0, 0, (int)model.NonFormID);

                    MailConstructor mailer = new MailConstructor(_context);
                    mailer.SetTemplate(MailTemplate.NonFormDocDecline);
                    mailer.SetValue("%approver%", model.Approver);
                    mailer.SetValue("%comment%", model.Comment);
                    mailer.AddFile(file);
                    await mailer.SendMail("Ваш документ был отклонен бухгалтером", nonFormDoc.Sender);
                }

                return StatusCode(StatusCodes.Status200OK, "Success");
            }
            catch (Exception ex)
            {
                await _context.CommonService.CreateLog(model.RequestID,
                                                clientId,
                                                Newtonsoft.Json.JsonConvert.SerializeObject(model),
                                                methodName,
                                                StatusCodes.Status500InternalServerError,
                                                "Ошибка: " + ex.Message + " StackException: " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpPost]
        public async Task<ActionResult<MetadataDTO>> AddOrUpdateExtDoc(dynamic metaObject)
        {
            string methodName = System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.Name;
            var converter = new ExpandoObjectConverter();
            var exObjExpandoObject = JsonConvert.DeserializeObject<ExpandoObject>(metaObject.ToString(), converter) as dynamic;
            MetadataDTO model = _context.CommonService.SetModel(exObjExpandoObject, new MetadataDTO());
            try
            {
                if (model == null)
                {
                    await _context.CommonService.CreateLog("",
                        0,
                        "",
                        methodName,
                        StatusCodes.Status400BadRequest,
                        "No entity provider");
                    return BadRequest("No entity provider");
                }

                if (model.Version1CExchange > 0)
                {
                    int version_1c = _context.Configuration["Version1CExchange"] == null ? -1 : Convert.ToInt32(_context.Configuration["Version1CExchange"]);
                    if (model.Version1CExchange < version_1c)
                    {
                        await _context.CommonService.CreateLog(model.RequestID,
                            model.ClientId,
                            Newtonsoft.Json.JsonConvert.SerializeObject(model),
                            methodName,
                            StatusCodes.Status400BadRequest,
                            "Need update 1C configuration version ( #err-10)");
                        return BadRequest("Need update 1C configuration version ( #err-10)");
                    }
                }
                var checkAuth = await CheckAuthorization();
                if ((checkAuth.res as ObjectResult).StatusCode != 200)
                    return checkAuth.res;
                model.ClientId = checkAuth.clientId;
                if (string.IsNullOrEmpty(model.RequestID))
                {
                    await _context.CommonService.CreateLog(model.RequestID,
                        model.ClientId,
                        Newtonsoft.Json.JsonConvert.SerializeObject(model),
                        methodName,
                        StatusCodes.Status400BadRequest,
                        "Field RequestID is required");
                    return BadRequest("Field RequestID is required");
                }
                if (model.Organization == null || string.IsNullOrEmpty(model.Organization.Ext_ID))
                {
                    await _context.CommonService.CreateLog(model.RequestID,
                        model.ClientId,
                        Newtonsoft.Json.JsonConvert.SerializeObject(model),
                        methodName,
                        StatusCodes.Status400BadRequest,
                        "Field Organization is required");
                    return BadRequest("Field Organization is required");
                }
                if (model.DocTypeId == 0)
                {
                    await _context.CommonService.CreateLog(model.RequestID,
                        model.ClientId,
                        Newtonsoft.Json.JsonConvert.SerializeObject(model),
                        methodName,
                        StatusCodes.Status400BadRequest,
                        "Field DocType is required");
                    return BadRequest("Field DocType is required");
                }
                if (string.IsNullOrEmpty(model.Source))
                {
                    await _context.CommonService.CreateLog(model.RequestID,
                        model.ClientId,
                        Newtonsoft.Json.JsonConvert.SerializeObject(model),
                        methodName,
                        StatusCodes.Status400BadRequest,
                        "Field Source is required");
                    return BadRequest("Field Source is required");
                }
                if (!SourceArray.Contains(model.Source))
                {
                    await _context.CommonService.CreateLog(model.RequestID,
                        model.ClientId,
                        Newtonsoft.Json.JsonConvert.SerializeObject(model),
                        methodName,
                        StatusCodes.Status400BadRequest,
                        "Field Source value incorrect");
                    return BadRequest("Field Source value incorrect");
                }
                /*if (model.DocDate == DateTime.MinValue)
                    return BadRequest("Field DocDate is required");*/

                var docType = await _context.DataBase.SingleAsync<DocType, DocType>(d => d.Id.Equals(model.DocTypeId));
                var checkModelResult = await CheckModelResult(model);
                if ((checkModelResult as ObjectResult).StatusCode != 200)
                    return checkModelResult;
                model.Modified = DateTime.Now;
                NonFormDocsDTO nonFormDoc = null;
                if (model.NonFormID != null)
                {
                    nonFormDoc = Ensol.CommonUtils.Common.GetNonFormDocByID(model.NonFormID.Value, _context.DbContext, model.ClientId);
                    if (nonFormDoc == null)
                    {
                        await _context.CommonService.CreateLog(model.RequestID,
                            model.ClientId,
                            Newtonsoft.Json.JsonConvert.SerializeObject(model),
                            methodName,
                            StatusCodes.Status400BadRequest,
                            "No NonFormDoc with id " + model.NonFormID);
                        return BadRequest("No NonFormDoc with id " + model.NonFormID);
                    }
                }
                MetadataDTO metadata = null;
                if (model.MetaID != null)
                {
                    metadata = Ensol.CommonUtils.Common.GetMetadataByID(model.MetaID.Value, _context.DbContext, model.ClientId);
                    if (metadata == null)
                    {
                        await _context.CommonService.CreateLog(model.RequestID,
                            model.ClientId,
                            Newtonsoft.Json.JsonConvert.SerializeObject(model),
                            methodName,
                            StatusCodes.Status400BadRequest,
                            "No Metadata document was found");
                        return BadRequest("No Metadata document was found");
                    }
                }
                else if (model.EDIId != null)
                {
                    metadata = await _context.DataBase.SingleAsync<Metadata, MetadataDTO>(m => m.EDIId.Equals(model.EDIId) && m.ClientId.Equals(model.ClientId), true);
                }
                else
                {
                    metadata = await _context.DataBase.SingleAsync<Metadata, MetadataDTO>(m => m.Ext_ID.Equals(model.Ext_ID) && m.ClientId.Equals(model.ClientId), true);
                }
                if (metadata != null)
                {
                    foreach (var key in ((IDictionary<String, Object>)exObjExpandoObject).Keys)
                    {
                        var prop = model.GetType().GetProperty(key);
                        if (prop == null)
                            continue;
                        var val = prop.GetValue(model, null);
                        metadata.GetType().GetProperty(key).SetValue(metadata, val, null);
                    }

                    if (await _context.DataBase.UpdateAsync<MetadataDTO, Metadata>(metadata))
                    {
                        await CheckAndSendEmail(docType, metadata, metadata.Id);
                        if (nonFormDoc == null && metadata.Binaries != null)
                        {
                            foreach (BinariesDTO binFile in metadata.Binaries)
                            {
                                if (string.IsNullOrEmpty(binFile.FileName) || string.IsNullOrEmpty(binFile.FileBase64))
                                    continue;
                                try
                                {
                                    var res = _context.CommonService.GetOriginalLengthInMB(binFile.FileBase64);
                                    if (res > 20)
                                    {
                                        await _context.CommonService.CreateLog(model.RequestID,
                                            model.ClientId,
                                            Newtonsoft.Json.JsonConvert.SerializeObject(model),
                                            methodName,
                                            StatusCodes.Status400BadRequest,
                                            "Max File size exceeded ( #err-20)");
                                        return BadRequest("Max File size exceeded ( #err-20)");
                                    }
                                }
                                catch
                                {
                                    await _context.CommonService.CreateLog(model.RequestID,
                                        model.ClientId,
                                        Newtonsoft.Json.JsonConvert.SerializeObject(model),
                                        methodName,
                                        StatusCodes.Status400BadRequest,
                                        "Can not parse File to Base64");
                                    return BadRequest("Can not parse File to Base64");
                                }
                                var file = await _context.DataBase.SingleAsync<DocFile, BinariesDTO>(f => f.MetaId.Equals(metadata.Id) && f.FileName.Equals(binFile.FileName));
                                await _context.CommonService.CreateOrUpdateFile(file, binFile, metadata.Id, metadata.ClientId, "meta");
                            }
                        }
                        return NoContent();
                    }
                    else
                    {
                        await _context.CommonService.CreateLog(model.RequestID,
                            model.ClientId,
                            Newtonsoft.Json.JsonConvert.SerializeObject(model),
                            methodName,
                            StatusCodes.Status400BadRequest,
                            "Could not update Metadata entity");
                        return BadRequest("Could not update Metadata entity");
                    }
                }
                /*if (model.Binaries == null || model.Binaries.Count == 0)
                    return BadRequest("No files to add to metadata");*/
                /*if (nonFormDoc == null && model.Binaries.Count == 0)
                    return BadRequest("File is required");*/

                if (nonFormDoc == null)
                {
                    foreach (BinariesDTO binFile in model.Binaries)
                    {
                        if (string.IsNullOrEmpty(binFile.FileBase64))
                        {
                            await _context.CommonService.CreateLog(model.RequestID,
                                model.ClientId,
                                Newtonsoft.Json.JsonConvert.SerializeObject(model),
                                methodName,
                                StatusCodes.Status400BadRequest,
                                "No binaries FileBase64 string");
                            return BadRequest("No binaries FileBase64 string");
                        }
                        if (string.IsNullOrEmpty(binFile.FileName))
                        {
                            await _context.CommonService.CreateLog(model.RequestID,
                                model.ClientId,
                                Newtonsoft.Json.JsonConvert.SerializeObject(model),
                                methodName,
                                StatusCodes.Status400BadRequest,
                                "No file name");
                            return BadRequest("No file name");
                        }
                        try
                        {
                            var res = _context.CommonService.GetOriginalLengthInMB(binFile.FileBase64);
                            if (res > 20)
                            {
                                await _context.CommonService.CreateLog(model.RequestID,
                                    model.ClientId,
                                    Newtonsoft.Json.JsonConvert.SerializeObject(model),
                                    methodName,
                                    StatusCodes.Status400BadRequest,
                                    "Max File size exceeded ( #err-20)");
                                return BadRequest("Max File size exceeded ( #err-20)");
                            }
                        }
                        catch
                        {
                            await _context.CommonService.CreateLog(model.RequestID,
                                model.ClientId,
                                Newtonsoft.Json.JsonConvert.SerializeObject(model),
                                methodName,
                                StatusCodes.Status400BadRequest,
                                "Can not parse File to Base64");
                            return BadRequest("Can not parse File to Base64");
                        }
                    }
                }

                if (docType != null)
                {
                    if (docType.Reestr.Equals("Input"))
                    {
                        model.Source = "Входящий";
                    }
                    else if (docType.Reestr.Equals("Realiz"))
                    {
                        model.Source = "Исходящий";
                    }
                }
                model.Created = DateTime.Now;
                model.ModifiedBy = "ExternalSystem";
                model.CreatedBy = "ExternalSystem";
                model.Ext_ID = model.Ext_ID;
                model.State = model.State ?? "Проведено в 1С";
                //model.Id = 0;
                var idMet = await _context.DataBase.CreateAsyncInt64<MetadataDTO, Metadata>(model);
                var dto = Ensol.CommonUtils.Common.GetMetadataByID(idMet, _context.DbContext, checkAuth.clientId);
                if (dto == null)
                {
                    await _context.CommonService.CreateLog(model.RequestID,
                        model.ClientId,
                        Newtonsoft.Json.JsonConvert.SerializeObject(model),
                        methodName,
                        StatusCodes.Status400BadRequest,
                        "Unable to add the entity");
                    return BadRequest("Unable to add the entity");
                }
                await CheckAndSendEmail(docType, model, idMet);
                if (nonFormDoc == null)
                {
                    foreach (BinariesDTO binFile in model.Binaries)
                    {
                        var file = await _context.DataBase.SingleAsync<DocFile, BinariesDTO>(f => f.MetaId.Equals(dto.Id) && f.FileName.Equals(binFile.FileName));
                        await _context.CommonService.CreateOrUpdateFile(file, binFile, dto.Id, model.ClientId, "meta");
                    }
                }
                else
                {
                    var extSett = await _context.DataBase.SingleAsync<ExtExchangeSetting, ExtExchangeSetting>(f => f.ClientId == model.ClientId);
                    if (extSett != null)
                    {
                        if (!string.IsNullOrEmpty(nonFormDoc.Sender) && extSett.NotifyEmailSender == true)
                        {
                            var DocName = dto.DocType + " " + dto.DocNumber + " контрагент " + dto.Contractor.Name;
                            var DocID = dto.Id.ToString();
                            var Settname = docType.Reestr;
                            var Doclink = "<a href='" + _context.Configuration["HttpClient_Address"] + "/newstyle/document/view?ItemId=" + DocID + "&SettName=" + Settname + "'>" + DocName + "</a>";
                            string ctName = dto.Contractor == null ? "" : dto.Contractor.Name;
                            string EventText = string.Format("Ваш документ {3} {0} на сумму {1} от {2} был обработан в 1С", dto.DocType, dto.Amount, ctName, Doclink);

                            await _context.CommonService.CreateUserEvent(nonFormDoc.Sender, EventText, DateTime.Now, 0, 0, (int)dto.Id);

                            MailConstructor mailer = new MailConstructor(_context);
                            mailer.SetTemplate(MailTemplate.NonFormDocApproval);
                            mailer.FillMetadataFields(dto, _context.DbContext);
                            await mailer.SendMail("Ваш документ был обработан бухгалтером", nonFormDoc.Sender);
                        }
                    }
                    var err = await _context.CommonService.MoveNonFormDocs("Meta", nonFormDoc.Id, (int)dto.Id, model.ClientId);
                    if (!string.IsNullOrEmpty(err))
                    {
                        await _context.CommonService.CreateLog(model.RequestID,
                                    model.ClientId,
                                    Newtonsoft.Json.JsonConvert.SerializeObject(model),
                                    methodName,
                                    StatusCodes.Status500InternalServerError,
                                    "Ошибка: " + err);
                        return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure " + err);
                    }
                }
                await _context.CommonService.CreateLog(model.RequestID,
                                model.ClientId,
                                Newtonsoft.Json.JsonConvert.SerializeObject(model),
                                methodName,
                                StatusCodes.Status201Created);

                var uri = _context.LinkGenerator.GetPathByAction("AddOrUpdateExtDoc", "Metadata");
                return Created(uri, dto);
            }
            catch (Exception ex)
            {
                await _context.CommonService.CreateLog(model.RequestID,
                                model.ClientId,
                                Newtonsoft.Json.JsonConvert.SerializeObject(model),
                                methodName,
                                StatusCodes.Status500InternalServerError,
                                "Ошибка: " + ex.Message + " StackException: " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        private async Task CheckAndSendEmail(DocType docType, MetadataDTO model, long id)
        {
            MailConstructor mailer = new MailConstructor(_context);
            string methodName = System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.Name;
            string user = string.Empty;
            string mailsubj = "Ваш счет был оплачен";
            model.Id = id;
            if (docType != null)
            {
                var DocName = model.DocType + " " + model.DocNumber + " контрагент " + model.Contractor.Name;
                var DocID = id.ToString();
                var Settname = docType.Reestr;
                var Doclink = "<a href='" + _context.Configuration["HttpClient_Address"] + "/newstyle/document/view?ItemId=" + DocID + "&SettName=" + Settname + "'>" + DocName + "</a>";
                if (docType.Name == "Счет исходящий" && !string.IsNullOrEmpty(model.NotifyUser) && model.Paid != true)
                {
                    user = model.NotifyUser;
                    string ctName = model.Contractor == null ? "" : model.Contractor.Name;
                    string EventText = string.Format("В 1С для вас был выпущен счет {0} на сумму {1}", Doclink, model.AmountToPay?.ToString("## ###.00"));
                    await _context.CommonService.CreateUserEvent(user, EventText, DateTime.Now, 0, 0, (int)model.Id);
                    mailer.SetTemplate(MailTemplate.OutgoingInvoiceCreated);
                    mailsubj = "Для Вас был подготовлен счет";
                }
                else if ((docType.Name == "Счет исходящий" || docType.Name == "Счет входящий") && model.Paid == true)
                {
                    var template = docType.Name == "Счет исходящий" && !string.IsNullOrEmpty(model.NotifyUser) ? "OutgoingInvoicePaid.html" : docType.Name == "Счет входящий" ? "IncomigInvoicePaid.html" : "";
                    if (!string.IsNullOrEmpty(template))
                    {
                        user = docType.Name == "Счет исходящий" ? model.NotifyUser : model.CreatedBy;
                        string EventText = string.Format("Ваш счет {0} на сумму {1} был оплачен.", Doclink, model.AmountToPay?.ToString("## ###.00"));
                        await _context.CommonService.CreateUserEvent(user, EventText, DateTime.Now, 0, 0, (int)model.Id);
                        mailer.SetTemplate("./wwwroot/src/EmailTemplates/" + template);
                    }
                }
                if (string.IsNullOrEmpty(user))
                    return;
                if (_context.CommonService.IsValidEmail(user))
                {
                    mailer.FillMetadataFields(model, _context.DbContext);
                    await mailer.SendMail(mailsubj, user);
                    await _context.CommonService.CreateLog(model.RequestID,
                                            model.ClientId,
                                            Newtonsoft.Json.JsonConvert.SerializeObject(model),
                                            methodName,
                                            StatusCodes.Status200OK,
                                            "Документ успешно отправлен по адресу " + user);
                }
                else
                {
                    await _context.CommonService.CreateLog(model.RequestID,
                                            model.ClientId,
                                            Newtonsoft.Json.JsonConvert.SerializeObject(model),
                                            methodName,
                                            StatusCodes.Status400BadRequest,
                                            "Ошибка: почтовый адрес " + user + " некорректен!");
                }
            }
        }


        [HttpPost]
        public async Task<ActionResult<MetadataDTO>> AddOrUpdateLocalDoc(dynamic metaObject)
        {
            string methodName = System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.Name;
            var converter = new ExpandoObjectConverter();
            var exObjExpandoObject = JsonConvert.DeserializeObject<ExpandoObject>(metaObject.ToString(), converter) as dynamic;
            MetadataDTO model = _context.CommonService.SetModel(exObjExpandoObject, new MetadataDTO());
            try
            {
                if (model == null)
                {
                    await _context.CommonService.CreateLog("",
                        0,
                        "",
                        methodName,
                        StatusCodes.Status400BadRequest,
                        "No entity provider");
                    return BadRequest("No entity provider");
                }
                if (model.Version1CExchange > 0)
                {
                    int version_1c = _context.Configuration["Version1CExchange"] == null ? -1 : Convert.ToInt32(_context.Configuration["Version1CExchange"]);
                    if (model.Version1CExchange < version_1c)
                    {
                        await _context.CommonService.CreateLog(model.RequestID,
                            model.ClientId,
                            Newtonsoft.Json.JsonConvert.SerializeObject(model),
                            methodName,
                            StatusCodes.Status400BadRequest,
                            "Need update 1C configuration version ( #err-10)");
                        return BadRequest("Need update 1C configuration version ( #err-10)");
                    }
                }
                var checkAuth = await CheckAuthorization();
                if ((checkAuth.res as ObjectResult).StatusCode != 200)
                    return checkAuth.res;
                model.ClientId = checkAuth.clientId;

                if (string.IsNullOrEmpty(model.RequestID))
                {
                    await _context.CommonService.CreateLog(model.RequestID,
                        model.ClientId,
                        Newtonsoft.Json.JsonConvert.SerializeObject(model),
                        methodName,
                        StatusCodes.Status400BadRequest,
                        "Field RequestID is required");
                    return BadRequest("Field RequestID is required");
                }
                if (model.Organization == null || string.IsNullOrEmpty(model.Organization.Ext_ID))
                {
                    await _context.CommonService.CreateLog(model.RequestID,
                        model.ClientId,
                        Newtonsoft.Json.JsonConvert.SerializeObject(model),
                        methodName,
                        StatusCodes.Status400BadRequest,
                        "Field Organization is required");
                    return BadRequest("Field Organization is required");
                }

                var checkModelResult = await CheckModelResult(model);
                if ((checkModelResult as ObjectResult).StatusCode != 200)
                {
                    await _context.CommonService.CreateLog(model.RequestID,
                        model.ClientId,
                        Newtonsoft.Json.JsonConvert.SerializeObject(model),
                        methodName,
                        (int)(checkModelResult as ObjectResult).StatusCode);
                    return checkModelResult;
                }

                model.Modified = DateTime.Now;
                NonFormDocsDTO nonFormDoc = null;
                if (model.NonFormID != null)
                {
                    nonFormDoc = Ensol.CommonUtils.Common.GetNonFormDocByID(model.NonFormID.Value, _context.DbContext, model.ClientId);
                    if (nonFormDoc == null)
                    {
                        await _context.CommonService.CreateLog(model.RequestID,
                            model.ClientId,
                            Newtonsoft.Json.JsonConvert.SerializeObject(model),
                            methodName,
                            StatusCodes.Status400BadRequest,
                            "No NonFormDoc with id " + model.NonFormID);
                        return BadRequest("No NonFormDoc with id " + model.NonFormID);
                    }
                }
                var metadata = await _context.DataBase.SingleAsync<Metadata, MetadataDTO>(m => m.Ext_ID.Equals(model.Ext_ID) && m.ClientId.Equals(model.ClientId));
                if (metadata != null)
                {
                    foreach (var key in ((IDictionary<String, Object>)exObjExpandoObject).Keys)
                    {
                        var prop = model.GetType().GetProperty(key);
                        if (prop == null)
                            continue;
                        var val = prop.GetValue(model, null);
                        metadata.GetType().GetProperty(key).SetValue(metadata, val, null);
                    }

                    if (await _context.DataBase.UpdateAsync<MetadataDTO, Metadata>(metadata))
                    {
                        if (nonFormDoc == null && metadata.Binaries != null)
                        {
                            foreach (BinariesDTO binFile in metadata.Binaries)
                            {
                                if (string.IsNullOrEmpty(binFile.FileName) || string.IsNullOrEmpty(binFile.FileBase64))
                                    continue;
                                try
                                {
                                    var res = _context.CommonService.GetOriginalLengthInMB(binFile.FileBase64);
                                    if (res > 20)
                                    {
                                        await _context.CommonService.CreateLog(model.RequestID,
                                            model.ClientId,
                                            Newtonsoft.Json.JsonConvert.SerializeObject(model),
                                            methodName,
                                            StatusCodes.Status400BadRequest,
                                            "Max File size exceeded ( #err-20)");
                                        return BadRequest("Max File size exceeded ( #err-20)");
                                    }
                                }
                                catch
                                {
                                    await _context.CommonService.CreateLog(model.RequestID,
                                        model.ClientId,
                                        Newtonsoft.Json.JsonConvert.SerializeObject(model),
                                        methodName,
                                        StatusCodes.Status400BadRequest,
                                        "Can not parse File to Base64");
                                    return BadRequest("Can not parse File to Base64");
                                }
                                var file = await _context.DataBase.SingleAsync<DocFile, BinariesDTO>(f => f.MetaId.Equals(metadata.Id) && f.FileName.Equals(binFile.FileName));
                                await _context.CommonService.CreateOrUpdateFile(file, binFile, metadata.Id, metadata.ClientId, "meta");
                            }
                        }
                        else
                        {
                            var file = await _context.DataBase.SingleAsync<DocFile, BinariesDTO>(f => f.NonFormDocId.Equals(nonFormDoc.Id));
                            if (file != null)
                            {
                                file.NonFormDocId = 0;
                                file.MetaId = metadata.Id;
                                await _context.DataBase.UpdateAsync<BinariesDTO, DocFile>(file);
                            }
                            await _context.DataBase.DeleteAsync<NonFormDocs>(d => d.Id.Equals(nonFormDoc.Id));
                        }
                        return NoContent();
                    }
                    else
                    {
                        await _context.CommonService.CreateLog(model.RequestID,
                            model.ClientId,
                            Newtonsoft.Json.JsonConvert.SerializeObject(model),
                            methodName,
                            StatusCodes.Status400BadRequest,
                            "Could not update Metadata entity");
                        return BadRequest("Could not update Metadata entity");
                    }
                }
                /*if (model.Binaries == null || model.Binaries.Count == 0)
                    return BadRequest("No files to add to metadata");*/
                /*if (nonFormDoc == null && model.Binaries.Count == 0)
                    return BadRequest("File is required");*/

                if (nonFormDoc == null)
                {
                    foreach (BinariesDTO binFile in model.Binaries)
                    {
                        if (string.IsNullOrEmpty(binFile.FileBase64))
                        {
                            await _context.CommonService.CreateLog(model.RequestID,
                                model.ClientId,
                                Newtonsoft.Json.JsonConvert.SerializeObject(model),
                                methodName,
                                StatusCodes.Status400BadRequest,
                                "No binaries FileBase64 string");
                            return BadRequest("No binaries FileBase64 string");
                        }
                        if (string.IsNullOrEmpty(binFile.FileName))
                        {
                            await _context.CommonService.CreateLog(model.RequestID,
                                model.ClientId,
                                Newtonsoft.Json.JsonConvert.SerializeObject(model),
                                methodName,
                                StatusCodes.Status400BadRequest,
                                "No file name");
                            return BadRequest("No file name");
                        }
                        try
                        {
                            var res = _context.CommonService.GetOriginalLengthInMB(binFile.FileBase64);
                            if (res > 20)
                            {
                                await _context.CommonService.CreateLog(model.RequestID,
                                    model.ClientId,
                                    Newtonsoft.Json.JsonConvert.SerializeObject(model),
                                    methodName,
                                    StatusCodes.Status400BadRequest,
                                    "Max File size exceeded ( #err-20)");
                                return BadRequest("Max File size exceeded ( #err-20)");
                            }
                        }
                        catch
                        {
                            await _context.CommonService.CreateLog(model.RequestID,
                                model.ClientId,
                                Newtonsoft.Json.JsonConvert.SerializeObject(model),
                                methodName,
                                StatusCodes.Status400BadRequest,
                                "Can not parse File to Base64");
                            return BadRequest("Can not parse File to Base64");
                        }
                    }
                }
                model.Source = "Внутренний";
                model.Created = DateTime.Now;
                model.ModifiedBy = "ExternalSystem";
                model.CreatedBy = "ExternalSystem";
                model.Ext_ID = model.Ext_ID;
                model.State = model.State ?? "Проведено в 1С";
                //model.Id = 0;
                var idMet = await _context.DataBase.CreateAsyncInt64<MetadataDTO, Metadata>(model);
                var dto = Ensol.CommonUtils.Common.GetMetadataByID(idMet, _context.DbContext, checkAuth.clientId);
                if (dto == null)
                {
                    await _context.CommonService.CreateLog(model.RequestID,
                        model.ClientId,
                        Newtonsoft.Json.JsonConvert.SerializeObject(model),
                        methodName,
                        StatusCodes.Status400BadRequest,
                       "Unable to add the entity");
                    return BadRequest("Unable to add the entity");
                }
                if (nonFormDoc == null)
                {
                    foreach (BinariesDTO binFile in model.Binaries)
                    {
                        var file = await _context.DataBase.SingleAsync<DocFile, BinariesDTO>(f => f.MetaId.Equals(dto.Id) && f.FileName.Equals(binFile.FileName));
                        await _context.CommonService.CreateOrUpdateFile(file, binFile, dto.Id, model.ClientId, "meta");
                    }
                }
                else
                {
                    var err = await _context.CommonService.MoveNonFormDocs("Meta", nonFormDoc.Id, (int)dto.Id, model.ClientId);
                    if (!string.IsNullOrEmpty(err))
                    {
                        await _context.CommonService.CreateLog(model.RequestID,
                                    model.ClientId,
                                    Newtonsoft.Json.JsonConvert.SerializeObject(model),
                                    methodName,
                                    StatusCodes.Status500InternalServerError,
                                    "Ошибка: " + err);
                        return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure " + err);
                    }
                }
                await _context.CommonService.CreateLog(model.RequestID,
                                model.ClientId,
                                Newtonsoft.Json.JsonConvert.SerializeObject(model),
                                methodName,
                                StatusCodes.Status201Created);

                var uri = _context.LinkGenerator.GetPathByAction("AddOrUpdateLocalDoc", "Metadata");
                return Created(uri, dto);
            }
            catch (Exception ex)
            {
                await _context.CommonService.CreateLog(model.RequestID,
                    model.ClientId,
                    Newtonsoft.Json.JsonConvert.SerializeObject(model),
                    methodName,
                    StatusCodes.Status500InternalServerError,
                    "Ошибка: " + ex.Message + " StackException: " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        public async Task<(int clientId, ActionResult res)> CheckAuthorization()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return (-1, StatusCode(StatusCodes.Status400BadRequest, "No Authorization header ( #err-80)"));
            var authHeader = Request.Headers["Authorization"][0];
            var token = string.Empty;
            if (authHeader.StartsWith("Bearer "))
            {
                token = authHeader.Substring("Bearer ".Length);
            }
            if (string.IsNullOrEmpty(token))
                return (-1, StatusCode(StatusCodes.Status400BadRequest, "No token received ( #err-30)"));
            var client = await _context.DataBase.SingleAsync<Client, ClientDTO>(c => c.Token.Equals(token));
            if (client == null)
                return (-1, StatusCode(StatusCodes.Status401Unauthorized, "Could not find any client for this token ( #err-40)"));
            var result = await _context.TokenService.CheckToken(token, client);
            if (!string.IsNullOrEmpty(result))
                return (-1, StatusCode(StatusCodes.Status400BadRequest, result));
            var checkQuota = _context.CommonService.CheckClientIsActive(client);
            if (!checkQuota.res)
                return (-1, StatusCode(StatusCodes.Status400BadRequest, checkQuota.err));
            return (client.Id, StatusCode(StatusCodes.Status200OK, string.Empty));
        }
        private async Task<ActionResult> CheckModelResult(MetadataDTO model)
        {
            var organization = await _context.DataBase.SingleAsync<Organization, OrganizationDTO>(o => o.Ext_ID.Equals(model.Organization.Ext_ID) && o.ClientId.Equals(model.ClientId));
            if (organization == null)
                organization = await _context.DataBase.SingleAsync<Organization, OrganizationDTO>(o => o.INN.Equals(model.Organization.INN) && o.KPP.Equals(model.Organization.KPP) && o.ClientId.Equals(model.ClientId));
            if (organization == null)
            {
                if (string.IsNullOrEmpty(model.Organization.Name))
                    return StatusCode(StatusCodes.Status400BadRequest, "Field Name in Organization list is required");
                model.Organization.ClientId = model.ClientId;
                //model.Organization.Id = 0;
                var idOrg = await _context.DataBase.CreateAsyncInt32<OrganizationDTO, Organization>(model.Organization);
                model.Organization.Id = idOrg;
            }
            else
            {
                model.Organization.Id = organization.Id;
                model.Organization.ClientId = model.ClientId;
                await _context.DataBase.UpdateAsync<OrganizationDTO, Organization>(model.Organization);
            }

            if (model.Contractor != null && !string.IsNullOrEmpty(model.Contractor.Ext_ID))
            {
                var contractor = await _context.DataBase.SingleAsync<Contractor, ContractorDTO>(o => o.Ext_ID.Equals(model.Contractor.Ext_ID) && o.Organization.Id.Equals(model.Organization.Id));
                if (contractor == null && model.Organization != null)
                    contractor = await _context.DataBase.SingleAsync<Contractor, ContractorDTO>(o => o.Organization.Id.Equals(model.Organization.Id) && o.INN.Equals(model.Contractor.INN) && o.KPP.Equals(model.Contractor.KPP));
                if (contractor == null)
                {
                    if (string.IsNullOrEmpty(model.Contractor.Name))
                        return BadRequest("Field Name in Contractor list is required");
                    model.Contractor.OrganizationId = model.Organization.Id;
                    //model.Contractor.Id = 0;
                    var ctrId = await _context.DataBase.CreateAsyncInt32<ContractorDTO, Contractor>(model.Contractor);
                    model.Contractor.Id = ctrId;
                }
                else
                {
                    model.Contractor.Id = contractor.Id;
                    model.Contractor.OrganizationId = model.Organization.Id;
                    await _context.DataBase.UpdateAsync<ContractorDTO, Contractor>(model.Contractor);
                }
            }

            if (model.Contract != null && !string.IsNullOrEmpty(model.Contract.Ext_ID))
            {
                var contract = await _context.DataBase.SingleAsync<Contract, ContractDTO>(o => o.Ext_ID.Equals(model.Contract.Ext_ID) && o.Organization.Id.Equals(model.Organization.Id));
                model.Contract.Modified = DateTime.Now;
                var docType = await _context.DataBase.GetAsync<DocType, DocType>(d => d.Name.Equals("Договор"));
                if (docType != null && docType.Count > 0)
                {
                    model.Contract.DocTypeId = docType[0].Id;
                }
                model.Contract.Organization = model.Organization;
                if (contract == null)
                {
                    if (string.IsNullOrEmpty(model.Contract.Name))
                        return StatusCode(StatusCodes.Status400BadRequest, "Field Name in Contract list is required");
                    if (model.Contractor != null && !string.IsNullOrEmpty(model.Contractor.Ext_ID))
                        model.Contract.Contractor = model.Contractor;
                    else if (model.Contract.Contractor == null)
                        return StatusCode(StatusCodes.Status400BadRequest, "Field Contractor in Contract list is required");

                    model.Contract.ClientId = model.ClientId;
                    model.Contract.Ext_ID = model.Contract.Ext_ID;
                    //model.Contract.Id = 0;
                    model.Contract.Created = DateTime.Now;
                    model.Contract.ModifiedBy = "ExternalSystem";
                    model.Contract.CreatedBy = "ExternalSystem";
                    model.Contract.State = model.Contract.State ?? "Проведено в 1С";
                    var idOrg = await _context.DataBase.CreateAsyncInt32<ContractDTO, Contract>(model.Contract);
                    model.Contract.Id = idOrg;
                }
                else
                {
                    model.Contract.Contractor = model.Contractor;
                    model.Contract.Id = contract.Id;
                    model.Contract.ClientId = model.ClientId;
                    model.Contract.Created = contract.Created;
                    model.Contract.ModifiedBy = contract.ModifiedBy;
                    model.Contract.CreatedBy = contract.CreatedBy;
                    model.Contract.State = contract.State;
                    await _context.DataBase.UpdateAsync<ContractDTO, Contract>(model.Contract);
                }
            }

            return StatusCode(StatusCodes.Status200OK, string.Empty);
        }
    }
}
