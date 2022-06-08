using ARCHIVE.COMMON.DTOModels;
using ARCHIVE.COMMON.DTOModels.Admin;
using ARCHIVE.COMMON.DTOModels.UI;
using ARCHIVE.COMMON.Entities;
using DATABASE.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudArchive.Services
{
    public interface ICommonService
    {
        (bool res, string err) CheckClientIsActive(ClientDTO client);
        float GetOriginalLengthInMB(string base64string);
        IUserService UserService { get; }
        Task<bool> CreateOrUpdateFile(BinariesDTO file, BinariesDTO binFile, long id, int clientId, string typeDoc);
        Task<List<DocFileDTO>> GetFilesById(string typedoc, long id);
        Task<bool> CreateLog(string requestId, int clientId, string json, string method, int statusCode, string error = "");
        Task<bool> CreateNonFormDocs(List<NonFormDocsDTO> arr);
        Task<bool> AddVersion(Versions model);
        string ReadFile(string FileName);
        bool IsValidEmail(string email);
        Task<bool> CreateUserEvent(string User, string EventText, DateTime eventDate, int ContractID = 0, int MetaID = 0, int NonFormDocId = 0);
        Task<bool> LogUserLogin(string User);
        TSource SetModel<TSource>(ExpandoObject metaObject, TSource model) where TSource : class;
        Dictionary<string, string> GetModelProps<TSource>(TSource model) where TSource : class;
        Task<string> MoveNonFormDocs(string settName, int idNf, int id, int clientId);
    }
}
