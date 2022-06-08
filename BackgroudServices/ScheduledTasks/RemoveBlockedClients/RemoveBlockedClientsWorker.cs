using ARCHIVE.COMMON.DTOModels.Admin;
using COMMON.Common.Services.StorageService;
using COMMON.Models;
using DATABASE.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace CloudArchive.ScheduledTasks
{
    public class RemoveBlockedClientsWorker
    {
        private readonly ClientDTO _clientDTO;
        private readonly SearchServiceDBContext _dbContext;
        private readonly IStorageService<StoredFile> _fileStorage;
        public RemoveBlockedClientsWorker(ClientDTO clientDTO, SearchServiceDBContext dbContext, IStorageService<StoredFile> fileStorage)
        {
            _clientDTO = clientDTO;
            _dbContext = dbContext;
            _fileStorage = fileStorage;
        }
        public void Process()
        {

            var metadataToDel = _dbContext.Metadatas.Where(t => t.ClientId == _clientDTO.Id);
            var filesOfMetadataToDel = _dbContext.Files.Where(t => metadataToDel.Select(e => e.Id).Contains(t.MetaId));
            var userTaskToDel = _dbContext.UsersTasks.Where(t => metadataToDel.Select(e => e.Id).Contains(t.MetadataId.Value));
            var nonFormToDel = _dbContext.NonFormDocs.Where(t => t.ClientId == _clientDTO.Id);
            var filesOfNonFormToDel = _dbContext.Files.Where(t => nonFormToDel.Select(e => e.Id).Contains(t.NonFormDocId.Value));
            var userToDel = _dbContext.AppUsers.Where(t => t.ClientId == _clientDTO.Id);
            var usersRolesToDel = _dbContext.UserRoles.Where(t => userToDel.Select(e => e.UserId).Contains(t.UserId));
            var aspNetUsersToDel = _dbContext.Users.Where(t => userToDel.Select(e => e.UserId).Contains(t.Id));
            var contractsToDel = _dbContext.Contracts.Where(t => t.ClientId == _clientDTO.Id);
            var filesOfContractsToDel = _dbContext.Files.Where(t => contractsToDel.Select(e => e.Id).Contains(t.ContractId));
            var versionsToDel = _dbContext.Versions.Where(t => contractsToDel.Select(e => e.Id).Contains(t.ContractId.Value));
            var versionMetaToDel = _dbContext.Versions.Where(t => metadataToDel.Select(e => e.Id).Contains(t.MetadataId.Value));
            var apiLogsToDel = _dbContext.ApiLogs.Where(t => t.ClientId == _clientDTO.Id);
            var clientToDel = _dbContext.Clients.Where(t => t.Id == _clientDTO.Id);
            var organizatonsToDel = _dbContext.Organizations.Where(t => t.ClientId == _clientDTO.Id);
            var contractorsToDel = _dbContext.Contractors.Where(t => organizatonsToDel.Select(e => e.Id).Contains(t.OrganizationId));
            var billingsToDel = _dbContext.Billings.Where(t => t.ClientId == _clientDTO.Id);
            var extConnectionsToDel = _dbContext.ExtConnections.Where(t => t.ClientId == _clientDTO.Id);
            var backgroundServiceLogsTodel = _dbContext.BackgroundServiceLogs.Where(t => t.Client == _clientDTO.Id);
            var extexchangeSettingsTodel = _dbContext.ExtExchangeSettings.Where(t => t.ClientId == _clientDTO.Id);
            var users = aspNetUsersToDel.Select(s => s.UserName).ToList();
            var reestrPerms = _dbContext.ReestrPerms.AsNoTracking().Where(r => users.Contains(r.User));
            var orgPerms = _dbContext.OrgPerms.AsNoTracking().Where(r => users.Contains(r.User));
            var projects = _dbContext.Projects.AsNoTracking().Where(r => r.ClientId == _clientDTO.Id);
            var projPerms = _dbContext.ProjectPerms.AsNoTracking().Where(r => users.Contains(r.User));
            var userSetts = _dbContext.UserSettings.Where(r => users.Contains(r.User));
            var favorites = _dbContext.Favorites.AsNoTracking().Where(r => users.Contains(r.UserName));
            var edi = _dbContext.EDISettings.AsNoTracking().Where(t => t.ClientID == _clientDTO.Id);
            var metalogs = _dbContext.SignaturesAndEDIEvents.Where(t => metadataToDel.Select(e => e.Id).Contains(t.MetaID));
            var contractlogs = _dbContext.SignaturesAndEDIEvents.Where(t => contractsToDel.Select(e => e.Id).Contains(t.ContractID));
            var subs = _dbContext.Substitutions.Where(t => t.ClientId == _clientDTO.Id);
            var fieldsettings = _dbContext.AdditionalFieldsMappings.Where(t => t.ClientId == _clientDTO.Id);
            var fields = _dbContext.AdditionalFields.Where(t => t.ClientId == _clientDTO.Id);
            _fileStorage.BatchDeleteAsync(filesOfNonFormToDel.Select(x => x.Id).ToList()).GetAwaiter().GetResult();
            _fileStorage.BatchDeleteAsync(filesOfMetadataToDel.Select(x => x.Id).ToList()).GetAwaiter().GetResult();
            _fileStorage.BatchDeleteAsync(filesOfContractsToDel.Select(x => x.Id).ToList()).GetAwaiter().GetResult();
            _dbContext.RemoveRange(extexchangeSettingsTodel);
            _dbContext.RemoveRange(reestrPerms);
            _dbContext.RemoveRange(orgPerms);
            _dbContext.RemoveRange(projects);
            _dbContext.RemoveRange(projPerms);
            _dbContext.RemoveRange(favorites);
            _dbContext.RemoveRange(userTaskToDel);
            _dbContext.RemoveRange(versionMetaToDel);
            _dbContext.RemoveRange(metadataToDel);
            _dbContext.RemoveRange(nonFormToDel);
            _dbContext.RemoveRange(extConnectionsToDel);
            _dbContext.RemoveRange(backgroundServiceLogsTodel);
            _dbContext.RemoveRange(versionsToDel);
            _dbContext.RemoveRange(contractsToDel);
            _dbContext.RemoveRange(contractorsToDel);
            _dbContext.RemoveRange(organizatonsToDel);
            _dbContext.RemoveRange(usersRolesToDel);
            _dbContext.RemoveRange(aspNetUsersToDel);
            _dbContext.RemoveRange(userToDel);
            _dbContext.RemoveRange(apiLogsToDel);
            _dbContext.RemoveRange(clientToDel);
            _dbContext.RemoveRange(billingsToDel);
            _dbContext.RemoveRange(edi);
            _dbContext.RemoveRange(userSetts);
            _dbContext.RemoveRange(metalogs);
            _dbContext.RemoveRange(contractlogs);
            _dbContext.RemoveRange(subs);
            _dbContext.RemoveRange(fieldsettings);
            _dbContext.RemoveRange(fields);
            _dbContext.SaveChanges();
        }
    }
}
