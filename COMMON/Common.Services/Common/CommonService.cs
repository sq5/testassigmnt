// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Newtonsoft.Json.Linq;
using ARCHIVE.COMMON.DTOModels;
using ARCHIVE.COMMON.DTOModels.Admin;
using ARCHIVE.COMMON.Entities;
using ARCHIVE.COMMON.Servises;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ARCHIVE.COMMON.DTOModels.UI;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using DATABASE.Context;
using DATABASE.Services;
using System.Collections;
using System.Dynamic;
using CloudArchive.Models;
using System.Reflection;
using COMMON.Common.Services.StorageService;
using COMMON.Models;
using COMMON.Common.Services.ContextService;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CloudArchive.Services
{
    public class CommonService : ICommonService
    {
        private readonly IAdminService _db;
        private readonly IStorageService<StoredFile> _fileStorage;
        public IServiceScopeFactory _serviceScopeFactory;
        public IUserService _usersvc;
        public IUserService UserService { get { return _usersvc; } }
        public SearchServiceDBContext _dbcontext;
        public CommonService(IAdminService db, IServiceScopeFactory serviceScopeFactory, IUserService usersvc, IStorageService<StoredFile> fileStorage, SearchServiceDBContext dbcontext)
        {
            _db = db;
            _serviceScopeFactory = serviceScopeFactory;
            _usersvc = usersvc;
            _fileStorage = fileStorage;
            _dbcontext = dbcontext;
        }

        public bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public string ReadFile(string FileName)
        {
            try
            {
                using (StreamReader reader = System.IO.File.OpenText(FileName))
                {
                    string fileContent = reader.ReadToEnd();
                    if (fileContent != null && fileContent != "")
                    {
                        return fileContent;
                    }
                }
            }
            catch (Exception ex)
            {
                //Log
                throw ex;
            }
            return null;
        }

        public (bool res, string err) CheckClientIsActive(ClientDTO client)
        {
            bool ret = false;
            string error = string.Empty;
            try
            {
                if (client.Blocked != null && (bool)client.Blocked)
                    error = "Client is blocked ( #err-50)";
                else if (client.StorageUsed >= (client.StorageQuota * 1024))
                    error = "Storage quota excceeded ( #err-60)";
                else
                    ret = true;

                return (ret, error);
            }
            catch (Exception ex)
            {
                return (ret, ex.Message + "StackTrace: " + ex.StackTrace);
            }
        }

        public float GetOriginalLengthInMB_Old(string base64string)
        {
            if (string.IsNullOrEmpty(base64string)) { return 0; }

            var characterCount = base64string.Length;
            var paddingCount = base64string.Substring(characterCount - 2, 2)
                                           .Count(c => c == '=');
            var gb = (float)Math.Round((((4 * characterCount / 3) + 3) & ~3) / 1024f / 1024f / 1024f, 2);
            return gb == 0 ? 0.01f : gb;
        }

        public float GetOriginalLengthInMB(string base64string)
        {
            byte[] sizeInBytes = Convert.FromBase64String(base64string);
            var fileSize = (float)Math.Round((sizeInBytes.Length / 1024.0F) / 1024.0F, 2);
            return fileSize;
        }

        public int GetOriginalLengthInBytes(float sizeInMb)
        {
            return (int)Math.Round(sizeInMb * 1024f * 1024f, MidpointRounding.AwayFromZero);
        }

        public async Task<bool> CreateOrUpdateFile(BinariesDTO file, BinariesDTO binFile, long id, int clientId, string typeDoc)
        {
            binFile.Modified = DateTime.Now;
            var fileSize = GetOriginalLengthInMB(binFile.FileBase64);
            var sizeInBytes = GetOriginalLengthInBytes(fileSize);
            bool res = false;
            if (file != null)
            {
                binFile.Created = file.Created;
                binFile.Id = file.Id;
                if (typeDoc == "meta")
                    binFile.MetaId = file.MetaId;
                if (typeDoc == "nonform")
                    binFile.NonFormDocId = file.NonFormDocId;
                if (typeDoc == "ctr")
                    binFile.ContractId = file.ContractId;
                binFile.FileSize = sizeInBytes;
                res = await _fileStorage.CreateOrUpdateAsync(binFile);
            }
            else
            {
                binFile.Created = DateTime.Now;
                if (typeDoc == "meta")
                    binFile.MetaId = id;
                if (typeDoc == "nonform")
                    binFile.NonFormDocId = Convert.ToInt32(id);
                if (typeDoc == "ctr")
                    binFile.ContractId = Convert.ToInt32(id);
                binFile.FileSize = sizeInBytes;
                res = await _fileStorage.CreateOrUpdateAsync(binFile);
            }
            return res;
        }

        public async Task<List<DocFileDTO>> GetFilesById(string typedoc, long id)
        {
            var files = typedoc == "Contracts" ?
                await _dbcontext.Files.AsNoTracking().Where(f => f.ContractId.Equals((int)id)).Select(s => new DocFileDTO { Id = s.Id, FileName = s.FileName, FileSize = (s.FileSize == null ? 0 : s.FileSize / 1024), Created = s.Created }).ToListAsync() :
                await _dbcontext.Files.AsNoTracking().Where(f => f.MetaId.Equals(id)).Select(s => new DocFileDTO { Id = s.Id, FileName = s.FileName, FileSize = (s.FileSize == null ? 0 : s.FileSize / 1024), Created = s.Created }).ToListAsync();
            foreach (var f in files)
            {
                if (_dbcontext.SignaturesAndEDIEvents.Where(x => x.FileID == f.Id).Any())
                    f.HasSignature = true;
                else
                    f.HasSignature = false;
            }
            if (files.Count() == 0)
                return null;
            return files.ToList();
        }

        public async Task<bool> CreateLog(string requestId, int clientId, string json, string method, int statusCode, string error = "")
        {
            bool ret = false;
            try
            {
                ApiLogDTO log = new ApiLogDTO();
                if (!string.IsNullOrEmpty(json))
                {
                    JObject jo = JObject.Parse(json);
                    if (jo.Property("Binaries") != null)
                        jo.Property("Binaries").Remove();
                    json = jo.ToString();
                    log.JSON = json;
                }
                else
                    log.JSON = "No json";
                log.ClientId = clientId;
                log.RequestID = requestId;
                log.Service = method;
                log.State = statusCode.ToString();
                log.Date = DateTime.Now;
                log.Exception = error;
                await _db.CreateAsyncInt32<ApiLogDTO, ApiLog>(log);
                return true;
            }
            catch (Exception)
            {
                return ret;
            }
        }

        public async Task<bool> AddVersion(Versions model)
        {
            bool ret = false;
            try
            {
                var result = (await _db.CreateAsyncInt32<Versions, Versions>(model)) > 0;
                return result;
            }
            catch (Exception)
            {
                return ret;
            }
        }

        public async Task<bool> CreateNonFormDocs(List<NonFormDocsDTO> arr)
        {
            bool ret = false;
            try
            {
                foreach (NonFormDocsDTO model in arr)
                {
                     var idDoc = await _db.CreateAsyncInt32<NonFormDocsDTO, NonFormDocs>(model);
                    foreach (BinariesDTO binFile in model.Binaries)
                    {
                        var res = await CreateOrUpdateFile(null, binFile, idDoc, model.ClientId, "nonform");
                        if (!res)
                            return false;
                    }
                    ret = true;
                }
                return ret;
            }
            catch
            {
                return ret;
            }
        }
        public async Task<string> MoveNonFormDocs(string settName, int idNf, int id, int clientId)
        {
            string err = "";
            string res = "";
            try
            {
                var file = await _db.SingleAsync<DocFile, BinariesDTO>(f => f.NonFormDocId == idNf);
                var nf = await _db.SingleAsync<NonFormDocs, NonFormDocsDTO>(x => x.Id == idNf);
                if (file == null || nf == null || nf.ClientId != clientId)
                {
                    return "No nonformdoc or nonformdocfile found";
                }
                //меняем ид у файла на док из meta/contr
                file.NonFormDocId = 0;
                if (settName == "Contracts")
                    file.ContractId = id;
                else
                    file.MetaId = id;
                res = _db.AddOrUpdateItemWOSave<BinariesDTO, DocFile>(file, false);
                if (!string.IsNullOrEmpty(res))
                    return "Error in save file. " + res;
                //если Meta - записываем sender/table part
                if (settName != "Contracts")
                {
                    try
                    {
                        var meta = Ensol.CommonUtils.Common.GetMetadataByID((long)id, _dbcontext);
                        string tblpart = string.IsNullOrEmpty(nf.OCRVerified) ? null : JsonConvert.DeserializeObject<List<MetadataDTO>>(nf.OCRVerified)[0].TablePart;
                        var users = _usersvc.GetUsersByClient(clientId);
                        if (!string.IsNullOrEmpty(nf.Sender) && users.Where(x => x.Email == nf.Sender).Any())
                            meta.NotifyUser = nf.Sender;
                        meta.TablePart = tblpart;
                        res = _db.AddOrUpdateItemWOSave<MetadataDTO, Metadata>(meta, false);
                        if (!string.IsNullOrEmpty(res))
                            return "Error in update meta. " + res;
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                }
                //записываем версию
                string SenderText = string.IsNullOrWhiteSpace(nf.Sender) ? "" : ". Отправитель: " + nf.Sender;
                int? metaID = null;
                int? ctrID = null;
                if (settName == "Contracts")
                    ctrID = id;
                else
                    metaID = id;
                Versions ver = new Versions()
                {
                    Action = "Документ загружен из e-mail/FTP" + SenderText,
                    Date = nf.Created,
                    User = "ExternalSystem",
                    MetadataId = metaID,
                    ContractId = ctrID,
                };
                res = _db.AddOrUpdateItemWOSave<Versions, Versions>(ver, true);
                if (!string.IsNullOrEmpty(res))
                    return "Error in update ver1. " + res;
                if (!string.IsNullOrWhiteSpace(nf.OCRState))
                {
                    Versions ver2 = new Versions()
                    {
                        Action = "Документ прошел распознавание со статусом: " + nf.OCRState,
                        Date = nf.Created.AddMinutes(20),
                        User = "ExternalSystem",
                        MetadataId = metaID,
                        ContractId = ctrID,
                    };
                    res = _db.AddOrUpdateItemWOSave<Versions, Versions>(ver2, true);
                    if (!string.IsNullOrEmpty(res))
                        return "Error in update ver2. " + res;
                }
                //удаляем nonform и сохраняем док
                res = await _db.SaveDBChangesAsync();
                if (!string.IsNullOrEmpty(res))
                    return "Error in save changes. " + res;
                await _db.DeleteAsync<NonFormDocs>(d => d.Id.Equals(idNf));
                return err;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public Task<bool> CreateLog(int clientId, string message, string source, string error = "")
        {
            return CreateLog("", clientId, message, source, 0, error);
        }

        public async Task<bool> CreateUserEvent(string User, string EventText, DateTime eventDate, int ContractID = 0, int MetaID = 0, int NonFormDocId = 0)
        {
            bool ret = false;
            try
            {
                UsersEventsDTO ev = new UsersEventsDTO();
                ev.User = User;
                ev.EventText = EventText;
                ev.EventDate = eventDate;
                ev.ContractID = ContractID;
                ev.MetaID = MetaID;
                ev.NonFormDocId = NonFormDocId;

                await _db.CreateAsyncInt32<UsersEventsDTO, UsersEvents>(ev);
                return true;
            }
            catch (Exception)
            {
                return ret;
            }
        }
        public async Task<bool> LogUserLogin(string User)
        {
            bool result = true;
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                try
                {
                    SearchServiceDBContext _dbContext = scope.ServiceProvider.GetRequiredService<SearchServiceDBContext>();
                    var UserDTO = await _usersvc.GetUserByEmailAsync(User);
                    var Client = _dbContext.Clients.Where(x => x.Id == UserDTO.ClientId).FirstOrDefault();
                    var userDB = _dbContext.Users.Where(x => x.Id == UserDTO.Id).FirstOrDefault();
                    userDB.LastLogin = DateTime.Now;
                    if (Client.LastLogin == null || DateTime.Today != Client.LastLogin.Value.Date)
                        Client.LastLogin = DateTime.Today;
                    _dbContext.SaveChanges();
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return result;
        }

        public TSource SetModel<TSource>(ExpandoObject metaObject, TSource model) where TSource : class
        {
            Dictionary<string, string> modelProps = GetModelProps(model);
            foreach (var property in (IDictionary<String, Object>)metaObject)
            {
                var key = modelProps.FirstOrDefault(m => m.Key == property.Key.ToLower()).Value;
                if (key == null)
                    continue;
                if (key.ToLower().Equals("buttondto"))
                    continue;
                var propType = model.GetType().GetProperty(key) == null ? null : model.GetType().GetProperty(key).PropertyType;
                var modelProp = model.GetType().GetProperty(key);
                var propSource = property.Value == null ? null : property.Value.GetType();
                if (propType == null)
                {
                    continue;
                }

                if (propSource == null)
                {
                    modelProp.SetValue(model, property.Value, null);
                    continue;
                }

                if (propSource == typeof(ExpandoObject))
                {
                    object o = Activator.CreateInstance(propType);
                    modelProp.SetValue(model, SetModel((ExpandoObject)property.Value, o), null);
                }
                else
                {
                    if (propType.Name == "Nullable`1" || propType.Name == "DateTime")
                    {
                        if (propType.FullName.Contains("DateTime"))
                        {
                            DateTime? dateT = Helper.ParseDateTime(property.Value.ToString());
                            if (dateT.HasValue)
                            {
                                if (dateT.Value.Minute == 0 && dateT.Value.Second == 0)
                                {
                                    dateT = dateT.Value.ToLocalTime();
                                    dateT = dateT.Value.AddHours(12 - dateT.Value.Hour);
                                }
                            }
                            modelProp.SetValue(model, dateT, null);
                        }
                        else if (propSource.Name != "Nullable`1")
                        {
                            Type t = Nullable.GetUnderlyingType(propType) ?? propType;
                            object safeValue = (property.Value == null) ? null : Convert.ChangeType(property.Value, t);
                            modelProp.SetValue(model, safeValue, null);
                        }
                    }
                    else
                    {
                        if (propType.Name == "Int32" && propSource.Name == "Int64")
                        {
                            modelProp.SetValue(model, Convert.ToInt32(property.Value), null);
                        }
                        else if (propType.Name == "ICollection`1" && propSource.Name == "List`1")
                        {
                            List<BinariesDTO> binArr = new List<BinariesDTO>();
                            var list = ConvertMysteriousObjectToList<object>(property.Value);
                            if (list != null)
                            {
                                foreach (var obj in list)
                                {
                                    object o = Activator.CreateInstance(typeof(BinariesDTO));
                                    binArr.Add((BinariesDTO)SetModel((ExpandoObject)obj, o));
                                }

                                modelProp.SetValue(model, binArr, null);
                            }
                        }
                        else
                        {
                            modelProp.SetValue(model, property.Value, null);
                        }
                    }
                }
            }
            return model;
        }

        public Dictionary<string, string> GetModelProps<TSource>(TSource model) where TSource : class
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            try
            {
                PropertyInfo[] properties = model.GetType().GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    res.Add(property.Name.ToLower(), property.Name);
                }
                return res;
            }
            catch (Exception)
            {
                return res;
            }
        }

        public static List<T> ConvertMysteriousObjectToList<T>(object input)
        {
            var enumerable = input as IEnumerable;
            if (enumerable == null)
                return null;
            return enumerable.Cast<T>().ToList();
        }
    }
}
