// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ARCHIVE.COMMON.DTOModels.Admin;
using ARCHIVE.COMMON.Entities;
using ARCHIVE.COMMON.Servises;

namespace DATABASE.Services
{
    public class UIReadService : IUIReadService
    {
        private readonly IDBReadService _db;
        private readonly IAdminService _dbAdmin;

        public UIReadService(IDBReadService db, IAdminService dbAdmin)
        {
            _db = db;
            _dbAdmin = dbAdmin;
        }

        public async Task<IEnumerable<Metadata>> GetMetadatas(int clientId)
        {
            _db.Include<Client, Organization>();
            _db.Include<Contractor, DocKind>();
            _db.Include<DocType>();
            var metadatas = await _db.GetAsync<Metadata>(m => m.ClientId.Equals(clientId));
            if (metadatas == null) return null;
            return metadatas;
        }

        public async Task<IEnumerable<DocFile>> GetFilesMeta(long metaId)
        {
            var files = await _db.GetAsync<DocFile>(f => f.MetaId.Equals(metaId));
            if (files == null) return null;
            return files;
        }

        public async Task<IEnumerable<DocFile>> GetFilesContract(int ctrId)
        {
            var files = await _db.GetAsync<DocFile>(f => f.ContractId.Equals(ctrId));
            if (files == null) return null;
            return files;
        }

        public async Task<IEnumerable<DocFile>> GetFilesNonFormDoc(int nfId)
        {
            var files = await _db.GetAsync<DocFile>(f => f.NonFormDocId.Equals(nfId));
            if (files == null) return null;
            return files;
        }

        public async Task<IEnumerable<Settings>> GetSettings(string settName)
        {
            var settings = await _db.GetAsync<Settings>(m => m.SettName.Equals(settName));
            if (settings == null) return null;
            return settings;
        }

        public async Task<Client> GetClient(string userId)
        {
            _db.Include<UserClient>();
            var userClient = await _db.SingleAsync<UserClient>(u => u.UserId.Equals(userId));
            if (userClient == null) return default;
            return userClient.Client;
        }

        public async Task<ExtConnectionDTO> GetExtConnection(int clientId, string type)
        {
            var extConn = await _dbAdmin.GetAsync<ExtConnection, ExtConnectionDTO>(u => u.ClientId.Equals(clientId) && u.Type.Equals(type));
            if (extConn == null) return default;
            return extConn.FirstOrDefault();
        }

        public async Task<Tariffs> GetTarif(int id)
        {
            var tarif = await _db.SingleAsync<Tariffs>(u => u.Id.Equals(id));
            if (tarif == null) return default;
            return tarif;
        }

        public async Task<IEnumerable<ApiLog>> GetLogs()
        {
            _db.Include<Client>();
            var logs = await _db.GetAsync<ApiLog>();
            if (logs == null) return default;
            return logs;
        }

        public async Task<IEnumerable<UserClient>> GetClients()
        {
            var clients = await _db.GetAsync<UserClient>();
            if (clients == null) return default;
            return clients;
        }

        public async Task<IEnumerable<Organization>> GetOrganizations()
        {
            _db.Include<Client>();
            var organizations = await _db.GetAsync<Organization>();
            if (organizations == null) return default;
            return organizations;
        }
        public async Task<IEnumerable<Contractor>> GetContractors()
        {
            _db.Include<Organization>();
            var contractors = await _db.GetAsync<Contractor>();
            if (contractors == null) return default;
            return contractors;
        }
    }
}
