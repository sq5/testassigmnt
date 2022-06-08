// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ARCHIVE.COMMON.DTOModels;
using ARCHIVE.COMMON.DTOModels.Admin;
using ARCHIVE.COMMON.Entities;
using COMMON.Common.Services.ContextService;
using ARCHIVE.COMMON.DTOModels.UI;
using Newtonsoft.Json;

namespace API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OutputController : Controller
    {
        private readonly IContextService _context;

        public OutputController(IContextService contextService)
        {
            _context = contextService;
        }

        [HttpGet()]
        public async Task<ActionResult<ClientDTO>> GetClientInfo()
        {
            var checkAuth = await CheckAuthorization();
            if ((checkAuth.res as ObjectResult).StatusCode != 200)
                return checkAuth.res;
            return checkAuth.clientDto;
        }

        [HttpGet("{DocId}/{RequestId}")]
        public async Task<ActionResult<KeyValuePair<int, List<BinariesWithFieldsDTO>>>> GetNonFormDoc(int docId, string requestId)
        {
            string methodName = System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.Name;
            int clientId = -1;
            try
            {
                var checkAuth = await CheckAuthorization();
                if ((checkAuth.res as ObjectResult).StatusCode != 200)
                    return checkAuth.res;
                if (checkAuth.clientDto.Blocked != null && (bool)checkAuth.clientDto.Blocked)
                    return BadRequest("Client is blocked ( #err-50)");
                clientId = checkAuth.clientDto.Id;
                
                var nonFormDocs = _context.DbContext.NonFormDocs.Where(x => x.ClientId.Equals(clientId)
                && x.Deleted != true && x.OCRState != "Отправка на распознавание" && x.OCRState != "На распознавании").
                Select(x => new NonFormDocs { Id = x.Id, Sender = x.Sender, OCRVerified = x.OCRVerified, OrganizationId = x.OrganizationId}).ToList();

                var files = _context.DbContext.Files.Where(e => e.NonFormDocId > 0).
                Select(x => new DocFile { FileName = x.FileName, NonFormDocId = x.NonFormDocId, FileSize = x.FileSize, Created = x.Created, Modified = x.Modified });

                var orgs = _context.DbContext.Organizations.AsNoTracking().Where(x => x.ClientId == clientId);
                if (docId > 0)
                    nonFormDocs = nonFormDocs.Where(x => x.Id > docId).ToList();
                int cntDocs = nonFormDocs.ToList().Count;
                var mainDocs = new List<OCRMetadataDTO>();
                foreach (var doc in nonFormDocs)
                {
                    if (!string.IsNullOrEmpty(doc.OCRVerified))
                    {
                        try
                        {
                            var docs = JsonConvert.DeserializeObject<List<OCRMetadataDTO>>(doc.OCRVerified);
                            if (docs.Count > 0)
                            {
                                var maindoc = docs[0];
                                maindoc.Id = doc.Id;
                                mainDocs.Add(maindoc);
                            }
                        }
                        catch (Exception ex)
                        {
                            await _context.CommonService.CreateLog(requestId,
                                clientId,
                                string.Empty,
                                methodName,
                                StatusCodes.Status500InternalServerError,
                                "Ошибка парсинга json NonFormID - " + doc.Id + " ошибка: " + ex.Message + " StackException: " + ex.StackTrace);
                        }
                    }
                }
                var res = (from d in nonFormDocs
                           join f in files
                                on d.Id equals f.NonFormDocId
                           join org in orgs
                           on d.OrganizationId equals org.Id
                                into orgList
                           from m in orgList.DefaultIfEmpty()
                           join meta in mainDocs
                                on d.Id equals meta.Id
                                into tempstorage
                           from dx in tempstorage.DefaultIfEmpty()
                           where d.Deleted != true
                           select new BinariesWithFieldsDTO
                           {
                               Id = (int)f.NonFormDocId,
                               FileName = f.FileName,
                               FileSize = f.FileSize ?? 0,
                               Created = f.Created,
                               Modified = f.Modified,
                               Sender = d.Sender,
                               Organization = m,
                               DocNumTaxInvoice = dx?.DocNumTaxInvoice,
                               DocDateTaxInvoice = dx?.DocDateTaxInvoice,
                               DocNumInvoice = dx?.DocNumInvoice,
                               DocDateInvoice = dx?.DocDateInvoice,
                               AmountToPay = dx?.AmountToPay,
                               DocNumber = dx?.DocNumber,
                               DocDate = dx?.DocDate,
                               Amount = dx?.Amount,
                               AmountWOVAT = dx?.AmountWOVAT,
                               VAT = dx?.VAT,
                               TablePart = dx?.TablePart,
                               DocTypeId = dx?.DocTypeId,
                               DocTypeName = dx?.DocType,
                               Contractor = dx?.Contractor,
                               Currency = dx?.Currency,
                               OCRtype = dx?.OCRtype,
                               AmountTotal = dx?.AmountTotal
                           }).OrderByDescending(a => a.Created).ToList();

                return new KeyValuePair<int, List<BinariesWithFieldsDTO>>(cntDocs, res);
            }
            catch (Exception ex)
            {
                await _context.CommonService.CreateLog(requestId,
                                clientId,
                                string.Empty,
                                methodName,
                                StatusCodes.Status500InternalServerError,
                                "Ошибка: " + ex.Message + " StackException: " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

        }

        [HttpGet("{DocId}/{RequestId}")]
        public async Task<ActionResult<KeyValuePair<int, List<object>>>> GetNewContracts(int docId, string requestId)
        {
            string methodName = System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.Name;
            int clientId = -1;
            try
            {
                var checkAuth = await CheckAuthorization();
                if ((checkAuth.res as ObjectResult).StatusCode != 200)
                    return checkAuth.res;
                if (checkAuth.clientDto.Blocked != null && (bool)checkAuth.clientDto.Blocked)
                {
                    await _context.CommonService.CreateLog(requestId,
                        0,
                       docId.ToString(),
                       methodName,
                        StatusCodes.Status400BadRequest,
                        "Client is blocked ( #err-50)");
                    return BadRequest("Client is blocked ( #err-50)");
                }
                clientId = checkAuth.clientDto.Id;


                var extExchSett = _context.DbContext.ExtExchangeSettings.AsNoTracking().FirstOrDefault(e => e.ClientId == clientId);
                if (extExchSett == null || extExchSett.SyncContracts != true)
                    return new KeyValuePair<int, List<object>>(0, null);
                var contractsArr = _context.DbContext.Contracts.AsNoTracking().Where(x => x.ClientId == clientId && x.Ext_ID == null && x.Deleted != true && x.Id > docId);
                if (extExchSett.SyncOnlyApprovedContracts == true)
                {
                    contractsArr = contractsArr.Where(x => (x.State == "Согласовано" || x.State == "Исполнено" || x.State == "Ознакомлено" || x.State == "Подписано"));
                }

                var res = (from c in contractsArr
                           select new
                           {
                               Id = c.Id,
                               DocNumber = c.DocNumber,
                               DocDate = c.DocDate == DateTime.MinValue ? null : c.DocDate,
                               Contractor = c.Contractor,
                               Organization = c.Organization,
                               Modified = c.Modified,
                               ModifiedBy = c.ModifiedBy,
                               Created = c.Created,
                               Type = c.Type,
                               Name = c.Name,
                               Amount = c.Amount,
                               AmountWOVAT = c.AmountWOVAT,
                               VAT = c.VAT,
                               Currency = c.Currency,
                               Comment = c.Comment,
                               Subject = c.Subject,
                               ValidityPeriod = c.ValidityPeriod == DateTime.MinValue ? null : c.ValidityPeriod,
                               State = c.State
                           })
                                  .OrderBy(a => a.Id)
                                  .ToList<object>();

                return new KeyValuePair<int, List<object>>(contractsArr.Count(), res);
            }
            catch (Exception ex)
            {
                await _context.CommonService.CreateLog(requestId,
                                clientId,
                                string.Empty,
                                methodName,
                                StatusCodes.Status500InternalServerError,
                                "Ошибка: " + ex.Message + " StackException: " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("{DocId}/{RequestId}")]
        public async Task<ActionResult<KeyValuePair<int, List<object>>>> GetNewInvoices(int docId, string requestId)
        {
            string methodName = System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.Name;
            int clientId = -1;
            try
            {
                var checkAuth = await CheckAuthorization();
                if ((checkAuth.res as ObjectResult).StatusCode != 200)
                    return checkAuth.res;
                if (checkAuth.clientDto.Blocked != null && (bool)checkAuth.clientDto.Blocked)
                {
                    await _context.CommonService.CreateLog(requestId,
                        0,
                       docId.ToString(),
                       methodName,
                        StatusCodes.Status400BadRequest,
                        "Client is blocked ( #err-50)");
                    return BadRequest("Client is blocked ( #err-50)");
                }
                clientId = checkAuth.clientDto.Id;
                var extSetts = _context.DbContext.ExtExchangeSettings.AsNoTracking().FirstOrDefault(e => e.ClientId == clientId);
                if (extSetts == null || extSetts.SyncInvoices != true)
                    return new KeyValuePair<int, List<object>>(0, null);
                IQueryable<Metadata> invoiceArr = _context.DbContext.Metadatas.AsNoTracking().Where(x => x.DocType != null && x.DocType.Name == "Счет входящий" && x.Deleted != true && x.ClientId == clientId && x.Ext_ID == null && x.Id > docId);
                if (extSetts.SyncOnlyApprovedInvoices == true)
                {
                    invoiceArr = invoiceArr.Where(x => (x.State == "Согласовано" || x.State == "Исполнено" || x.State == "Ознакомлено" || x.State == "Подписано"));
                }
                var metaArr = (from d in invoiceArr
                               select new
                               {
                                   Id = d.Id,
                                   Source = d.Source,
                                   DocNumber = d.DocNumber,
                                   DocDate = d.DocDate,
                                   VAT = d.VAT,
                                   AmountToPay = d.AmountToPay,
                                   AmountWOVAT = d.AmountWOVAT,
                                   Currency = d.Currency,
                                   Operation = d.Operation,
                                   Contract = d.Contract,
                                   State = d.State,
                                   Contractor = d.Contractor,
                                   Organization = d.Organization,
                                   Comment = d.Comment
                               })
                                  .OrderBy(a => a.Id)
                                  .ToList<object>();
                return new KeyValuePair<int, List<object>>(invoiceArr.Count(), metaArr);
            }
            catch (Exception ex)
            {
                await _context.CommonService.CreateLog(requestId,
                                clientId,
                                string.Empty,
                                methodName,
                                StatusCodes.Status500InternalServerError,
                                "Ошибка: " + ex.Message + " StackException: " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("{DocId}/{RequestId}")]
        public async Task<ActionResult<KeyValuePair<int, List<object>>>> GetNewInputs(int docId, string requestId)
        {
            string methodName = System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.Name;
            int clientId = -1;
            try
            {
                var checkAuth = await CheckAuthorization();
                if ((checkAuth.res as ObjectResult).StatusCode != 200)
                    return checkAuth.res;
                if (checkAuth.clientDto.Blocked != null && (bool)checkAuth.clientDto.Blocked)
                {
                    await _context.CommonService.CreateLog(requestId,
                        0,
                       docId.ToString(),
                       methodName,
                        StatusCodes.Status400BadRequest,
                        "Client is blocked ( #err-50)");
                    return BadRequest("Client is blocked ( #err-50)");
                }
                clientId = checkAuth.clientDto.Id;
                var extSetts = _context.DbContext.ExtExchangeSettings.AsNoTracking().FirstOrDefault(e => e.ClientId == clientId);
                if (extSetts == null || extSetts.SyncInputs != true)
                    return new KeyValuePair<int, List<object>>(0, null);
                IQueryable<Metadata> inputsarr = _context.DbContext.Metadatas.AsNoTracking().Where(x => x.DocType != null && x.DocType.Name == "Поступление" && x.Deleted != true && x.ClientId == clientId && x.Ext_ID == null && x.Id > docId);
                if (extSetts.SyncOnlyApprovedInputs == true)
                {
                    inputsarr = inputsarr.Where(x => (x.State == "Согласовано" || x.State == "Исполнено" || x.State == "Ознакомлено" || x.State == "Подписано"));
                }
                var metaArr = (from d in inputsarr
                               select new
                               {
                                   Id = d.Id,
                                   Source = d.Source,
                                   DocNumber = d.DocNumber,
                                   DocDate = d.DocDate,
                                   VAT = d.VAT,
                                   AmountToPay = d.AmountToPay,
                                   AmountWOVAT = d.AmountWOVAT,
                                   Amount = d.Amount,
                                   DocDateInvoice = d.DocDateInvoice,
                                   DocDateTaxInvoice = d.DocDateTaxInvoice,
                                   DocNumTaxInvoice = d.DocNumTaxInvoice,
                                   DocNumInvoice = d.DocNumInvoice,
                                   Currency = d.Currency,
                                   Operation = d.Operation,
                                   Contract = d.Contract,
                                   State = d.State,
                                   Contractor = d.Contractor,
                                   Organization = d.Organization,
                                   Comment = d.Comment,
                                   TablePart = d.TablePart,
                               })
                                  .OrderBy(a => a.Id)
                                  .ToList<object>();
                return new KeyValuePair<int, List<object>>(inputsarr.Count(), metaArr);
            }
            catch (Exception ex)
            {
                await _context.CommonService.CreateLog(requestId,
                                clientId,
                                string.Empty,
                                methodName,
                                StatusCodes.Status500InternalServerError,
                                "Ошибка: " + ex.Message + " StackException: " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet()]
        public async Task<ActionResult<List<object>>> GetClientUsers()
        {
            string methodName = System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.Name;
            int clientId = -1;
            try
            {
                var checkAuth = await CheckAuthorization();
                if ((checkAuth.res as ObjectResult).StatusCode != 200)
                    return checkAuth.res;
                if (checkAuth.clientDto.Blocked != null && (bool)checkAuth.clientDto.Blocked)
                {
                    await _context.CommonService.CreateLog("",
                        checkAuth.clientDto.Id,
                       "",
                       methodName,
                        StatusCodes.Status400BadRequest,
                        "Client is blocked ( #err-50)");
                    return BadRequest("Client is blocked ( #err-50)");
                }
                clientId = checkAuth.clientDto.Id;

                var users = _context.DbContext.Users.AsNoTracking()
                .Select(user => new UserDTO
                {
                    Email = user.Email,
                    Blocked = user.Blocked,
                    ClientId = !_context.DbContext.AppUsers.Any(u => u.UserId == user.Id) ? -1 : _context.DbContext.Clients.FirstOrDefault(c => c.Id == _context.DbContext.AppUsers.FirstOrDefault(u => u.UserId == user.Id).ClientId).Id,
                    UserName = user.DisplayName
                });
                var usrArr = (from d in users
                              where d.ClientId == clientId
                              select new
                              {
                                  Email = d.Email,
                                  Blocked = d.Blocked,
                                  UserName = d.UserName
                              })
                                  .OrderBy(a => a.Email)
                                  .ToList<object>();
                return usrArr;
            }
            catch (Exception ex)
            {
                await _context.CommonService.CreateLog("",
                                clientId,
                                string.Empty,
                                methodName,
                                StatusCodes.Status500InternalServerError,
                                "Ошибка: " + ex.Message + " StackException: " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        public async Task<(ClientDTO clientDto, ActionResult res)> CheckAuthorization()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return (null, StatusCode(StatusCodes.Status400BadRequest, "No Authorization header ( #err-80)"));
            var authHeader = Request.Headers["Authorization"][0];
            var token = string.Empty;
            if (authHeader.StartsWith("Bearer "))
            {
                token = authHeader.Substring("Bearer ".Length);
            }

            if (string.IsNullOrEmpty(token))
                return (null, StatusCode(StatusCodes.Status400BadRequest, "No token received ( #err-30)"));

            var client = await _context.DataBase.SingleAsync<Client, ClientDTO>(c => c.Token.Equals(token));
            if (client == null)
                return (null, StatusCode(StatusCodes.Status401Unauthorized, "Could not find any client for this token ( #err-40)"));
            var result = await _context.TokenService.CheckToken(token, client);
            if (!string.IsNullOrEmpty(result))
                return (null, StatusCode(StatusCodes.Status400BadRequest, result));
            ClientDTO dto = new ClientDTO()
            {
                Id = client.Id,
                UsersQuota = client.UsersQuota,
                UsersUsed = client.UsersUsed,
                StorageQuota = client.StorageQuota,
                StorageUsed = client.StorageUsed,
                Blocked = client.Blocked
            };
            return (dto, StatusCode(StatusCodes.Status200OK, string.Empty));
        }
    }
}
