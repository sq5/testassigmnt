// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ARCHIVE.COMMON.DTOModels;
using ARCHIVE.COMMON.DTOModels.Admin;
using ARCHIVE.COMMON.DTOModels.UI;
using ARCHIVE.COMMON.Entities;
using COMMON.Common.Services.ContextService;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IContextService _context;
        public FileController(IContextService contextService)
        {
            _context = contextService;
        }
        [Route("[action]/{id}/{settname}")]
        [HttpGet()]
        public async Task<ActionResult<FileCollection>> GetFileByExtIntId(string id, string settname)
        {
            string methodName = System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.Name;
            if (string.IsNullOrEmpty(id))
            {
                await _context.CommonService.CreateLog("",
                    0,
                    id,
                    methodName,
                    StatusCodes.Status400BadRequest,
                    "Id is required");
                return BadRequest("Id is required");
            }
            var checkAuth = await CheckAuthorization();
            if ((checkAuth.res as ObjectResult).StatusCode != 200)
                return checkAuth.res;
            List<BinariesDTO> files = null;
            int idint = 0;
            bool isint = int.TryParse(id, out idint);
            //return error if id not int
            if (!isint && settname != "ExtID")
            {
                await _context.CommonService.CreateLog("",
                0,
                id,
                methodName,
                StatusCodes.Status400BadRequest,
                "Id not parsed");
                return BadRequest("Id is not int");
            }
            //search by ext id
            if (settname == "ExtID")
            {
                if (files == null)
                {
                    var meta = await _context.DataBase.SingleAsync<Metadata, MetadataDTO>(m => m.Ext_ID.Equals(id) && m.ClientId.Equals(checkAuth.clientId));
                    if (meta != null)
                    {
                        files = await _context.DataBase.GetAsync<DocFile, BinariesDTO>(f => f.MetaId == meta.Id);
                    }
                }
                if (files == null)
                {
                    var contract = await _context.DataBase.SingleAsync<Contract, ContractDTO>(c => c.Ext_ID.Equals(id) && c.ClientId.Equals(checkAuth.clientId));
                    if (contract != null)
                    {
                        files = await _context.DataBase.GetAsync<DocFile, BinariesDTO>(f => f.ContractId == contract.Id);
                    }
                }
            }
            //search by nonform id
            if (settname == "NonFormID")
            {
                NonFormDocsDTO nonFormDoc = null;
                nonFormDoc = Ensol.CommonUtils.Common.GetNonFormDocByID(idint, _context.DbContext, checkAuth.clientId);
                if (nonFormDoc != null)
                {
                    files = await _context.DataBase.GetAsync<DocFile, BinariesDTO>(f => f.NonFormDocId == nonFormDoc.Id);
                }
            }
            //search by metaid
            if (settname == "MetaID")
            {
                var meta = Ensol.CommonUtils.Common.GetMetadataByID((long)idint, _context.DbContext, checkAuth.clientId);
                if (meta != null)
                {
                    files = await _context.DataBase.GetAsync<DocFile, BinariesDTO>(f => f.MetaId == meta.Id);
                }
            }
            //search by contract id
            if (settname == "ContractID")
            {
                var ctr = Ensol.CommonUtils.Common.GetContractByID(idint, _context.DbContext, checkAuth.clientId);
                if (ctr != null)
                {
                    files = await _context.DataBase.GetAsync<DocFile, BinariesDTO>(f => f.ContractId == ctr.Id);
                }
            }
            if (files != null && files.Count > 0)
            {
                FileCollection coll = new FileCollection();
                foreach (var file in files)
                {
                    FileInfo fi = new FileInfo();
                    fi.FileBase64 = Convert.ToBase64String(_context.FileStorage.GetFileAsync(file).GetAwaiter().GetResult());
                    fi.FileName = file.FileName;
                    coll.Binaries.Add(fi);
                }
                return coll;
            }
            await _context.CommonService.CreateLog("",
                checkAuth.clientId,
                id,
                methodName,
                StatusCodes.Status400BadRequest,
                "File not found ( #err-70)");
            return BadRequest("File not found ( #err-70)");
        }
        [Route("[action]/{id}")]
        [HttpGet()]
        public async Task<ActionResult<FileCollection>> GetFileByExtId(string id)
        {
            string methodName = System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.Name;
            if (string.IsNullOrEmpty(id))
            {
                await _context.CommonService.CreateLog("",
                    0,
                    id,
                    methodName,
                    StatusCodes.Status400BadRequest,
                    "Id is required");
                return BadRequest("Id is required");
            }
            var checkAuth = await CheckAuthorization();
            if ((checkAuth.res as ObjectResult).StatusCode != 200)
                return checkAuth.res;
            List<BinariesDTO> files = null;
            int i = -1;
            bool isInt = int.TryParse(id, out i);
            if (isInt)
            {
                try
                {
                    var nonFormDoc = Ensol.CommonUtils.Common.GetNonFormDocByID(i, _context.DbContext, checkAuth.clientId);
                    if (nonFormDoc != null)
                    {
                        files = await _context.DataBase.GetAsync<DocFile, BinariesDTO>(f => f.NonFormDocId == nonFormDoc.Id);
                    }
                }
                catch { }
            }
            if (files == null)
            {
                var meta = await _context.DataBase.SingleAsync<Metadata, MetadataDTO>(m => m.Ext_ID.Equals(id) && m.ClientId.Equals(checkAuth.clientId));
                if (meta != null)
                {
                    files = await _context.DataBase.GetAsync<DocFile, BinariesDTO>(f => f.MetaId == meta.Id);
                }
            }
            if (files == null)
            {
                var contract = await _context.DataBase.SingleAsync<Contract, ContractDTO>(c => c.Ext_ID.Equals(id) && c.ClientId.Equals(checkAuth.clientId));
                if (contract != null)
                {
                    files = await _context.DataBase.GetAsync<DocFile, BinariesDTO>(f => f.ContractId == contract.Id);
                }
            }
            if (files != null && files.Count > 0)
            {
                FileCollection coll = new FileCollection();
                foreach (var file in files)
                {
                    FileInfo fi = new FileInfo();
                    fi.FileBase64 = Convert.ToBase64String(_context.FileStorage.GetFileAsync(file).GetAwaiter().GetResult());
                    fi.FileName = file.FileName;
                    coll.Binaries.Add(fi);
                }
                return coll;
            }
            await _context.CommonService.CreateLog("",
                checkAuth.clientId,
                id,
                methodName,
                StatusCodes.Status400BadRequest,
                "File not found ( #err-70)");
            return BadRequest("File not found ( #err-70)");
        }
        [Route("[action]/{id}")]
        [HttpGet()]
        public async Task<ActionResult<FileCollection>> GetMetaFileById(int id)
        {
            string methodName = System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.Name;
            var checkAuth = await CheckAuthorization();
            if ((checkAuth.res as ObjectResult).StatusCode != 200)
                return checkAuth.res;
            List<BinariesDTO> files = null;
            var meta = Ensol.CommonUtils.Common.GetMetadataByID(id, _context.DbContext, checkAuth.clientId);
            if (meta != null)
            {
                files = await _context.DataBase.GetAsync<DocFile, BinariesDTO>(f => f.MetaId == meta.Id);
            }

            if (files != null && files.Count > 0)
            {
                FileCollection coll = new FileCollection();
                foreach (var file in files)
                {
                    FileInfo fi = new FileInfo();
                    fi.FileBase64 = Convert.ToBase64String(_context.FileStorage.GetFileAsync(file).GetAwaiter().GetResult());
                    fi.FileName = file.FileName;
                    coll.Binaries.Add(fi);
                }
                return coll;
            }
            await _context.CommonService.CreateLog("",
                checkAuth.clientId,
                id.ToString(),
                methodName,
                StatusCodes.Status400BadRequest,
                "File not found ( #err-70)");
            return BadRequest("Files not found ( #err-70)");
        }
        [Route("[action]/{id}")]
        [HttpGet()]
        public async Task<ActionResult<FileCollection>> GetContractFileById(int id)
        {
            string methodName = System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.Name;
            var checkAuth = await CheckAuthorization();
            if ((checkAuth.res as ObjectResult).StatusCode != 200)
                return checkAuth.res;
            List<BinariesDTO> files = null;
            var ctr = Ensol.CommonUtils.Common.GetContractByID(id, _context.DbContext, checkAuth.clientId);
            if (ctr != null)
            {
                files = await _context.DataBase.GetAsync<DocFile, BinariesDTO>(f => f.ContractId == ctr.Id);
            }

            if (files != null && files.Count > 0)
            {
                FileCollection coll = new FileCollection();
                foreach (var file in files)
                {
                    FileInfo fi = new FileInfo();
                    fi.FileBase64 = Convert.ToBase64String(_context.FileStorage.GetFileAsync(file).GetAwaiter().GetResult());
                    fi.FileName = file.FileName;
                    coll.Binaries.Add(fi);
                }
                return coll;
            }
            await _context.CommonService.CreateLog("",
                checkAuth.clientId,
                id.ToString(),
                methodName,
                StatusCodes.Status400BadRequest,
                "File not found ( #err-70)");
            return BadRequest("Files not found ( #err-70)");
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

            return (client.Id, StatusCode(StatusCodes.Status200OK, string.Empty));
        }
        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
        public class FileCollection
        {
            public List<FileInfo> Binaries { get; set; } = new List<FileInfo>();
        }
        public class FileInfo
        {
            public string FileName { get; set; }
            public string FileBase64 { get; set; }
        }
    }
}
