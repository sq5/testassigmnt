using ARCHIVE.COMMON.DTOModels.Files;
using ARCHIVE.COMMON.Entities;
using COMMON.Admin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CloudArchive.Services.PermissionService
{
    public interface IPermissionService
    {
        Task<Permission> GetUserCardPerms(string settname, long id,  AppUser appuser, int taskid);
        List<string> GetDeniedReestrsByUser(string email);
        List<int> GetDeniedProjectsByUser(string email);
        List<int> GetDeniedOrgsByUser(string email);
    }
}
