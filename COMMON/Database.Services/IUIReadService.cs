// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ARCHIVE.COMMON.DTOModels.Admin;
using ARCHIVE.COMMON.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATABASE.Services
{
    public interface IUIReadService
    {
        Task<Client> GetClient(string userId);
        Task<Tariffs> GetTarif(int id);
        Task<IEnumerable<Metadata>> GetMetadatas(int clientId);
        Task<IEnumerable<Settings>> GetSettings(string settName);
        Task<IEnumerable<DocFile>> GetFilesMeta(long metaId);
        Task<IEnumerable<DocFile>> GetFilesContract(int ctrId);
        Task<IEnumerable<DocFile>> GetFilesNonFormDoc(int nfId);
        Task<IEnumerable<ApiLog>> GetLogs();
        Task<IEnumerable<UserClient>> GetClients();
        Task<IEnumerable<Organization>> GetOrganizations();
        Task<IEnumerable<Contractor>> GetContractors();
        Task<ExtConnectionDTO> GetExtConnection(int clientId, string type);
    }
}
