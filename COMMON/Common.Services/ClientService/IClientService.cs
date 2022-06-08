using ARCHIVE.COMMON.DTOModels.Files;
using ARCHIVE.COMMON.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudArchive.Services.ClientService
{
    public interface IClientService
    {
        int GetUsedSpaceBy(int clientId);

        List<DocFile> GetAllFilesFor(int clientId);
        List<DocFile> GetAllMetadataFilesFor(int clientId);
        List<DocFile> GetAllNonFormDocsFilesFor(int clientId);
        List<DocFile> GetAllContractsFilesFor(int clientId);
        List<FileBO> GetListContractsFilesFor(int clientId, int take, int skip);
        List<FileBO> GetListMetadatasFilesFor(int clientId, int take, int skip);
        List<FileBO> GetListNonFormFilesFor(int clientId, int take, int skip);
        (int countOfMetadatas, int countOfContracts, int total) GetCountOfAllFilesFor(int clientId);
        List<FileBO> GetListAllFilesFor(int clientId, int take, int skip);
    }
}
