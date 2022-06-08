// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ARCHIVE.COMMON.DTOModels.UI;
using ARCHIVE.COMMON.Entities;
using Microsoft.AspNetCore.Routing;
using ARCHIVE.COMMON.DTOModels.Admin;
using ARCHIVE.COMMON.DTOModels;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using COMMON.Utilities;
using COMMON.Common.Services.ContextService;

namespace API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ContractController : ControllerBase
    {
        private readonly IContextService _context;
        //private readonly string[] ContractTypeArray = { "С поставщиком", "С покупателем" };

        public ContractController(IContextService contextService)
        {
            _context = contextService;
        }

        [HttpPost]
        public async Task<ActionResult<ContractDTO>> AddOrUpdateContract(dynamic ctrObject)
        {
            string methodName = System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.Name;
            var converter = new ExpandoObjectConverter();
            var exObjExpandoObject = JsonConvert.DeserializeObject<ExpandoObject>(ctrObject.ToString(), converter) as dynamic;
            ContractDTO model = _context.CommonService.SetModel(exObjExpandoObject, new ContractDTO());
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
                int version_1c = _context.Configuration["Version1CExchange"] == null ? -1 : Convert.ToInt32(_context.Configuration["Version1CExchange"]);
                if (model.Version1CExchange > 0)
                {
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
                if (!Request.Headers.ContainsKey("Authorization"))
                {
                    await _context.CommonService.CreateLog(model.RequestID,
                        model.ClientId,
                        Newtonsoft.Json.JsonConvert.SerializeObject(model),
                        methodName,
                        StatusCodes.Status400BadRequest,
                        "No Authorization header ( #err-80)");
                    return BadRequest("No Authorization header ( #err-80)");
                }
                var authHeader = Request.Headers["Authorization"][0];
                var token = string.Empty;
                if (authHeader.StartsWith("Bearer "))
                {
                    token = authHeader.Substring("Bearer ".Length);
                }

                if (string.IsNullOrEmpty(token))
                {
                    await _context.CommonService.CreateLog(model.RequestID,
                        model.ClientId,
                        Newtonsoft.Json.JsonConvert.SerializeObject(model),
                        methodName,
                        StatusCodes.Status400BadRequest,
                        "No token received ( #err-30) ");
                    return BadRequest("No token received ( #err-30) ");
                }
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

                var client = await _context.DataBase.SingleAsync<Client, ClientDTO>(c => c.Token.Equals(token));
                if (client == null)
                {
                    await _context.CommonService.CreateLog(model.RequestID,
                        model.ClientId,
                        Newtonsoft.Json.JsonConvert.SerializeObject(model),
                        methodName,
                        StatusCodes.Status401Unauthorized,
                        "Could not find any client for this token ( #err-40)");
                    return StatusCode(StatusCodes.Status401Unauthorized, "Could not find any client for this token ( #err-40)");
                }
                var checkClient = _context.CommonService.CheckClientIsActive(client);
                if (!checkClient.res)
                {
                    await _context.CommonService.CreateLog(model.RequestID,
                        model.ClientId,
                        Newtonsoft.Json.JsonConvert.SerializeObject(model),
                        methodName,
                        StatusCodes.Status400BadRequest,
                        checkClient.err);
                    return BadRequest(checkClient.err);
                }
                var result = await _context.TokenService.CheckToken(token, client);
                if (!string.IsNullOrEmpty(result))
                {
                    await _context.CommonService.CreateLog(model.RequestID,
                        model.ClientId,
                        Newtonsoft.Json.JsonConvert.SerializeObject(model),
                        methodName,
                        StatusCodes.Status400BadRequest,
                        result);
                    return BadRequest(result);
                }

                if (string.IsNullOrEmpty(model.Name))
                {
                    await _context.CommonService.CreateLog(model.RequestID,
                        model.ClientId,
                        Newtonsoft.Json.JsonConvert.SerializeObject(model),
                        methodName,
                        StatusCodes.Status400BadRequest,
                        "Field Name is required");
                    return BadRequest("Field Name is required");
                }
                /*if (string.IsNullOrEmpty(model.Type))
                    return BadRequest("Field Type is required");*/
                /*if (!string.IsNullOrEmpty(model.Type))
                    if (!ContractTypeArray.Contains(model.Type))
                        return BadRequest("Field Type value incorrect");*/
                if (model.Contractor == null || string.IsNullOrEmpty(model.Contractor.Ext_ID))
                {
                    await _context.CommonService.CreateLog(model.RequestID,
                        model.ClientId,
                        Newtonsoft.Json.JsonConvert.SerializeObject(model),
                        methodName,
                        StatusCodes.Status400BadRequest,
                        "Field Contractor is required");
                    return BadRequest("Field Contractor is required");
                }
                if (model.Organization == null || string.IsNullOrEmpty(model.Organization.Ext_ID))
                {
                    await _context.CommonService.CreateLog(model.RequestID,
                        model.ClientId,
                        Newtonsoft.Json.JsonConvert.SerializeObject(model),
                        methodName,
                        StatusCodes.Status400BadRequest,
                        "Field Contractor is required");
                    return BadRequest("Field Contractor is required");
                }

                model.ClientId = client.Id;

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

                var organization = await _context.DataBase.SingleAsync<Organization, OrganizationDTO>(o => o.Ext_ID.Equals(model.Organization.Ext_ID) && o.ClientId.Equals(model.ClientId));
                if (organization == null)
                    organization = await _context.DataBase.SingleAsync<Organization, OrganizationDTO>(o => o.INN.Equals(model.Organization.INN) && o.KPP.Equals(model.Organization.KPP) && o.ClientId.Equals(model.ClientId));
                if (organization == null)
                {
                    if (string.IsNullOrEmpty(model.Organization.Name))
                    {
                        await _context.CommonService.CreateLog(model.RequestID,
                            model.ClientId,
                            Newtonsoft.Json.JsonConvert.SerializeObject(model),
                            methodName,
                            StatusCodes.Status400BadRequest,
                            "Field Name in Organization list is required");
                        return BadRequest("Field Name in Organization list is required");
                    }
                    model.Organization.ClientId = client.Id;
                    //model.Organization.Id = 0;
                    var idOrg = await _context.DataBase.CreateAsyncInt32<OrganizationDTO, Organization>(model.Organization);
                    model.Organization.Id = idOrg;
                }
                else
                {
                    model.Organization.Id = organization.Id;
                    model.Organization.ClientId = client.Id;
                    await _context.DataBase.UpdateAsync<OrganizationDTO, Organization>(model.Organization);
                }
                model.OrganizationId = model.Organization.Id;

                var contractor = await _context.DataBase.SingleAsync<Contractor, ContractorDTO>(o => o.Ext_ID.Equals(model.Contractor.Ext_ID) && o.Organization.Ext_ID.Equals(model.Organization.Ext_ID) && o.Organization.ClientId.Equals(model.ClientId));
                if (contractor == null && model.Organization != null)
                    contractor = await _context.DataBase.SingleAsync<Contractor, ContractorDTO>(o => o.Organization.Id.Equals(model.Organization.Id) && o.INN.Equals(model.Contractor.INN) && o.KPP.Equals(model.Contractor.KPP));
                if (contractor == null)
                {
                    if (string.IsNullOrEmpty(model.Contractor.Name))
                    {
                        await _context.CommonService.CreateLog(model.RequestID,
                            model.ClientId,
                            Newtonsoft.Json.JsonConvert.SerializeObject(model),
                            methodName,
                            StatusCodes.Status400BadRequest,
                            "Field Name in Contractor list is required");
                        return BadRequest("Field Name in Contractor list is required");
                    }
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

                model.Modified = DateTime.Now;

                ContractDTO contract = null;
                if (model.ContractID != null)
                {
                    contract = Ensol.CommonUtils.Common.GetContractByID(model.ContractID.Value, _context.DbContext, model.ClientId);
                    if (contract == null)
                    {
                        await _context.CommonService.CreateLog(model.RequestID,
                            model.ClientId,
                            Newtonsoft.Json.JsonConvert.SerializeObject(model),
                            methodName,
                            StatusCodes.Status400BadRequest,
                            "No Contract document was found");
                        return BadRequest("No Contract document was found");
                    }
                }
                else
                {
                    contract = await _context.DataBase.SingleAsync<Contract, ContractDTO>(c => c.Ext_ID.Equals(model.Ext_ID) && c.ClientId.Equals(model.ClientId) && c.OrganizationId.Equals(model.Organization.Id), true);
                }
                if (contract != null)
                {
                    foreach (var key in ((IDictionary<String, Object>)exObjExpandoObject).Keys)
                    {
                        var prop = model.GetType().GetProperty(key);
                        if (prop == null)
                            continue;
                        var val = prop.GetValue(model, null);
                        contract.GetType().GetProperty(key).SetValue(contract, val, null);
                    }

                    if (await _context.DataBase.UpdateAsync<ContractDTO, Contract>(contract))
                    {
                        if (nonFormDoc == null)
                        {
                            if (contract.Binaries != null)
                            {
                                foreach (BinariesDTO binFile in model.Binaries)
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
                                    catch (Exception exc)
                                    {
                                        await _context.CommonService.CreateLog(model.RequestID,
                                            model.ClientId,
                                            Newtonsoft.Json.JsonConvert.SerializeObject(model),
                                            methodName,
                                            StatusCodes.Status500InternalServerError,
                                            "Ошибка: " + exc.Message + " StackException: " + exc.StackTrace);
                                        return BadRequest("Can not parse File to Base64");
                                    }
                                    var file = await _context.DataBase.SingleAsync<DocFile, BinariesDTO>(f => f.ContractId.Equals(contract.Id) && f.FileName.Equals(binFile.FileName));
                                    await _context.CommonService.CreateOrUpdateFile(file, binFile, contract.Id, contract.ClientId, "ctr");
                                }
                            }

                        }
                        else
                        {
                            var file = await _context.DataBase.SingleAsync<DocFile, BinariesDTO>(f => f.NonFormDocId.Equals(nonFormDoc.Id));
                            if (file != null)
                            {
                                file.NonFormDocId = 0;
                                file.ContractId = contract.Id;
                                await _context.DataBase.UpdateAsync<BinariesDTO, DocFile>(file);
                            }
                            await _context.DataBase.DeleteAsync<NonFormDocs>(d => d.Id.Equals(nonFormDoc.Id));
                        }
                        await _context.CommonService.CreateLog(model.RequestID,
                                    model.ClientId,
                                    Newtonsoft.Json.JsonConvert.SerializeObject(model),
                                    methodName,
                                    StatusCodes.Status204NoContent);
                        return NoContent();
                    }
                    else
                    {
                        await _context.CommonService.CreateLog(model.RequestID,
                                    model.ClientId,
                                    Newtonsoft.Json.JsonConvert.SerializeObject(model),
                                    methodName,
                                    StatusCodes.Status400BadRequest, "Could not update Contract entity");
                        return BadRequest("Could not update Contract entity");
                    }
                }
                /*if (model.Binaries == null || model.Binaries.Count == 0)
                    return BadRequest("No files to add to metadata");
                if (nonFormDoc == null && model.Binaries.Count == 0)
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
                                        StatusCodes.Status400BadRequest, "No binaries FileBase64 string");
                            return BadRequest("No binaries FileBase64 string");
                        }
                        if (string.IsNullOrEmpty(binFile.FileName))
                        {
                            await _context.CommonService.CreateLog(model.RequestID,
                                        model.ClientId,
                                        Newtonsoft.Json.JsonConvert.SerializeObject(model),
                                        methodName,
                                        StatusCodes.Status400BadRequest, "No file name");
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
                                            StatusCodes.Status400BadRequest, "Max File size exceeded ( #err-20)");
                                return BadRequest("Max File size exceeded ( #err-20)");
                            }
                        }
                        catch (Exception ex)
                        {
                            await _context.CommonService.CreateLog(model.RequestID,
                                model.ClientId,
                                Newtonsoft.Json.JsonConvert.SerializeObject(model),
                                methodName,
                                StatusCodes.Status500InternalServerError,
                                "Ошибка: " + ex.Message + " StackException: " + ex.StackTrace);
                            return BadRequest("Can not parse File to Base64");
                        }
                    }
                }
                var docType = await _context.DataBase.GetAsync<DocType, DocType>(d => d.Name.Equals("Договор"));
                if (docType != null && docType.Count > 0)
                {
                    model.DocTypeId = docType[0].Id;
                }
                model.ModifiedBy = "ExternalSystem";
                model.CreatedBy = "ExternalSystem";
                model.Created = DateTime.Now;
                model.Ext_ID = model.Ext_ID;
                model.State = model.State ?? "Проведено в 1С";
                //model.Id = 0;
                var idCtr = await _context.DataBase.CreateAsyncInt32<ContractDTO, Contract>(model);
                var dto = Ensol.CommonUtils.Common.GetContractByID(idCtr, _context.DbContext);
                if (dto == null)
                {
                    await _context.CommonService.CreateLog(model.RequestID,
                        model.ClientId,
                        Newtonsoft.Json.JsonConvert.SerializeObject(model),
                        methodName,
                        StatusCodes.Status500InternalServerError,
                        "Unable to add the entity");
                    return BadRequest("Unable to add the entity");
                }
                if (nonFormDoc == null)
                {
                    foreach (BinariesDTO binFile in model.Binaries)
                    {
                        var file = await _context.DataBase.SingleAsync<DocFile, BinariesDTO>(f => f.ContractId.Equals(dto.Id) && f.FileName.Equals(binFile.FileName));
                        await _context.CommonService.CreateOrUpdateFile(file, binFile, dto.Id, model.ClientId, "ctr");
                    }
                }
                else
                {
                    var extSett = await _context.DataBase.SingleAsync<ExtExchangeSetting, ExtExchangeSetting>(f => f.ClientId == model.ClientId);
                    if (extSett != null)
                    {
                        if (!string.IsNullOrEmpty(nonFormDoc.Sender) && extSett.NotifyEmailSender == true)
                        {
                            MailConstructor mailer = new MailConstructor(_context.CommonService, _context.EmailSender, _context.Configuration);
                            mailer.SetTemplate(MailTemplate.NonFormDocApproval);
                            mailer.FillContractFields(dto);
                            await mailer.SendMail("Ваш документ был обработан бухгалтером", nonFormDoc.Sender);
                        }
                    }
                    var err = await _context.CommonService.MoveNonFormDocs("Contracts", nonFormDoc.Id, (int)dto.Id, model.ClientId);
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
                var uri = _context.LinkGenerator.GetPathByAction("AddOrUpdateContract", "Contract");
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
    }
}
