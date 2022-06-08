// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ARCHIVE.COMMON.DTOModels.UI;
using ARCHIVE.COMMON.Entities;
using COMMON.Admin;
using DATABASE.Context;
using DATABASE.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudArchive.Services.PermissionService
{
    public class PermissionService : IPermissionService
    {
        readonly Permission _perms = new Permission();
        private readonly IUIReadService _readsvc;
        private readonly SearchServiceDBContext _dBContext;
        private readonly IUserService _userService;

        public PermissionService(IUIReadService readsvc, SearchServiceDBContext dBContext, IUserService userService)
        {
            _readsvc = readsvc;
            _dBContext = dBContext;
            _userService = userService;
        }
        public async Task<Permission> GetUserCardPerms(string settname, long id, AppUser appuser, int taskid)
        {
            try
            {
                var userID = appuser.Id;
                var useremail = appuser.Email;
                var user = await _userService.GetUserAsync(userID);
                bool isClientAdmin = user.ClientAdmin;
                var client = await _readsvc.GetClient(userID);
                int clientId = client.Id;
                DocData doc = new DocData();
                doc.GetDocData(settname, id, clientId, _dBContext);
                //проверка на то что док относится к правильному клиенту
                if ((settname == "Contracts" && doc.contract == null) || (settname != "Contracts" && doc.meta == null))
                {
                    SetError("Документ не найден!");
                    return _perms;
                }
                //проверка на доступ на чтение к реестрам/организациям/проектам
                if (client.UnicPerms == true)
                {
                    if (doc.prjID != 0 && GetDeniedProjectsByUser(useremail).Contains(doc.prjID))
                    {
                        SetError("У Вас нет доступам к документам по проекту " + doc.prjName + "!");
                        return _perms;
                    }
                    if (doc.orgID != 0 && GetDeniedOrgsByUser(useremail).Contains(doc.orgID))
                    {
                        SetError("У Вас нет доступам к документам организации " + doc.OrgName + "!");
                        return _perms;
                    }
                    if ((taskid == 0) && doc.createdBy != useremail && GetDeniedReestrsByUser(useremail).Contains(settname))
                    {
                        string settnameru = _dBContext.DocTypes.AsNoTracking().Where(x => x.Reestr == settname).FirstOrDefault().ReestrRu;
                        SetError("У Вас нет доступа на документы из реестра " + settnameru + "!");
                        return _perms;
                    }
                }
                _perms.Read = true;
                _perms.Write = false;
                _perms.FileWrite = false;
                _perms.Error = "";
                //Проверка на права редактирования
                //если Диадок/Сбис - нельзя редактировать никому
                //if (!string.IsNullOrEmpty(doc.ediID))
                //    return _perms;
                //если 1С - можно только редактировать файлы Клиент Админу
                if (!string.IsNullOrEmpty(doc.ext_ID) && isClientAdmin)
                {
                    _perms.FileWrite = true;
                    return _perms;
                }
                //если Новый - все
                if (doc.state == "Новый")
                {
                    _perms.Write = true;
                    _perms.FileWrite = true;
                    return _perms;
                }
                //если на согласовании/подписании - исполнитель + Клиент админ
                if ((doc.state == "На согласовании" || doc.state == "На подписании" || doc.state == "На исполнении") && (taskid != 0 || isClientAdmin))
                {
                    _perms.Write = true;
                    _perms.FileWrite = true;
                    return _perms;
                }
                //если на отклонено/прервано - создатель + инициатор процесса + Клиент админ
                if ((doc.state == "Отклонено" || doc.state == "Прервано"))
                {
                    if (doc.createdBy == useremail || isClientAdmin)
                    {
                        _perms.Write = true;
                        _perms.FileWrite = true;
                        return _perms;
                    }
                    IQueryable<UsersTasks> starttask;
                    if (settname == "Contracts")
                        starttask = _dBContext.UsersTasks.AsNoTracking().Where(x => x.ContractId == id && x.Users == useremail && x.Order == 0);
                    else
                        starttask = _dBContext.UsersTasks.AsNoTracking().Where(x => x.MetadataId == id && x.Users == useremail && x.Order == 0);
                    if (starttask.Any())
                    {
                        _perms.Write = true;
                        _perms.FileWrite = true;
                        return _perms;
                    }
                }
                if (isClientAdmin)
                {
                    _perms.Write = true;
                    _perms.FileWrite = true;
                }
                return _perms;
            }
            catch (Exception ex)
            {
                SetError(ex.Message + " StackTrace " + ex.StackTrace);
                return _perms;
            }
        }
        public List<string> GetDeniedReestrsByUser(string email)
        {
            List<string> reestrDenied = new List<string>();
            try
            {
                reestrDenied = _dBContext.ReestrPerms.AsNoTracking().Where(r => r.User.Equals(email)).Select(x => x.DeniedReestr).ToList(); ;
                return reestrDenied;
            }
            catch (Exception)
            {
                return reestrDenied;
            }
        }
        public List<int> GetDeniedOrgsByUser(string email)
        {
            List<int> deniedorgs = new List<int>();
            try
            {
                deniedorgs = _dBContext.OrgPerms.AsNoTracking().Where(r => r.User.Equals(email)).Select(x => x.OrganizationId).ToList();
                return deniedorgs;
            }
            catch (Exception)
            {
                return deniedorgs;
            }
        }
        public List<int> GetDeniedProjectsByUser(string email)
        {
            List<int> projectsDenied = new List<int>();
            try
            {
                projectsDenied = _dBContext.ProjectPerms.AsNoTracking().Where(r => r.User.Equals(email)).Select(x => x.ProjectId).ToList();
                return projectsDenied;
            }
            catch (Exception)
            {
                return projectsDenied;
            }
        }
        private void SetError(string error)
        {
            _perms.Error = error;
            _perms.Read = false;
            _perms.Write = false;
            _perms.FileWrite = false;
        }
    }

}
public class DocData
{
    public int orgID = 0;
    public int prjID = 0;
    public string prjName = "";
    public string OrgName = "";
    public string ext_ID;
    public string ediID;
    public string createdBy = "";
    public string state = "";
    public ContractDTO contract;
    public MetadataDTO meta;
    public void GetDocData(string settname, long id, int clientId, SearchServiceDBContext _dBContext)
    {
        if (settname == "Contracts")
        {
            contract = Ensol.CommonUtils.Common.GetContractByID((int)id, _dBContext, clientId);
            if (contract != null)
            {
                orgID = contract.Organization == null ? orgID : contract.Organization.Id;
                OrgName = contract.Organization == null ? OrgName : contract.Organization.Name;
                prjID = contract.Project == null ? prjID : contract.Project.Id;
                prjName = contract.Project == null ? prjName : contract.Project.Name;
                createdBy = contract.CreatedBy;
                ext_ID = contract.Ext_ID;
                ediID = contract.EDIId;
                state = contract.State;
            }
        }
        else
        {
            meta = Ensol.CommonUtils.Common.GetMetadataByID(id, _dBContext, clientId);
            if (meta != null)
            {
                orgID = meta.Organization == null ? orgID : meta.Organization.Id;
                OrgName = meta.Organization == null ? OrgName : meta.Organization.Name;
                prjID = meta.Project == null ? prjID : meta.Project.Id;
                prjName = meta.Project == null ? prjName : meta.Project.Name;
                createdBy = meta.CreatedBy;
                ext_ID = meta.Ext_ID;
                ediID = meta.EDIId;
                state = meta.State;
            }
        }
    }
}
