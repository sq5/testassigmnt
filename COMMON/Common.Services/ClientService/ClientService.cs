using ARCHIVE.COMMON.DTOModels.Files;
using ARCHIVE.COMMON.DTOModels.UI;
using ARCHIVE.COMMON.Entities;
using COMMON.Common.Services.StorageService;
using COMMON.Models;
using DATABASE.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudArchive.Services.ClientService
{
    public class ClientService : IClientService
    {
        private readonly SearchServiceDBContext _dbContext;
        private readonly IStorageService<StoredFile> _fileStorage;

        public ClientService(SearchServiceDBContext dbContext, IStorageService<StoredFile> fileStorage)
        {
            _dbContext = dbContext;
            _fileStorage = fileStorage;
        }


        public List<DocFile> GetAllFilesFor(int clientId)
        {
            List<DocFile> docFiles = new List<DocFile>();
            var c = GetAllContractsFilesFor(clientId);
            var n = GetAllNonFormDocsFilesFor(clientId);
            var m = GetAllMetadataFilesFor(clientId);

            docFiles.AddRange(c);
            docFiles.AddRange(n);
            docFiles.AddRange(m);

            return docFiles;
        }

        public List<DocFile> GetAllContractsFilesFor(int clientId)
        {
            var c = (from contracts in _dbContext.Contracts
                     join file in _dbContext.Files
                     on contracts.Id equals file.ContractId
                     where contracts.ClientId == clientId && file.FileName != null
                     select new DocFile
                     {
                         Id = (int)file.Id,
                         FileName = file.FileName,
                         FileSize = file.FileSize

                     }).ToList();
            return c;
        }

        public (int countOfMetadatas, int countOfContracts, int total) GetCountOfAllFilesFor(int clientId)
        {

            var m = (from metadata in _dbContext.Metadatas
                     join file in _dbContext.Files
                     on metadata.Id equals file.MetaId
                     where metadata.ClientId == clientId && metadata.Deleted != true
                     select file.Id
                     ).Count();
            var c = (from contracts in _dbContext.Contracts
                     join file in _dbContext.Files
                     on contracts.Id equals file.ContractId
                     where contracts.ClientId == clientId && file.FileName != null && contracts.Deleted != true
                     select file.Id).Count();

            return (m, c, m + c);

        }

        public List<DocFile> GetAllMetadataFilesFor(int clientId)
        {

            var m = (from metadata in _dbContext.Metadatas
                     join file in _dbContext.Files
                     on metadata.Id equals file.MetaId
                     where metadata.ClientId == clientId && file.FileName != null
                     select new DocFile
                     {
                         Id = (int)file.Id,
                         FileName = file.FileName,
                         FileSize = file.FileSize

                     }).ToList();
            return m;

        }

        public List<DocFile> GetAllNonFormDocsFilesFor(int clientId)
        {

            var n = (from nonformdocs in _dbContext.NonFormDocs
                     join file in _dbContext.Files
                      on nonformdocs.Id equals file.NonFormDocId
                     where nonformdocs.ClientId == clientId && file.FileName != null
                     select new DocFile
                     {
                         Id = (int)file.Id,
                         FileName = file.FileName,
                         FileSize = file.FileSize

                     }).ToList();
            return n;
        }

        public int GetUsedSpaceBy(int clientId)
        {
            var meta = _dbContext.Metadatas.AsNoTracking().Where(t => t.ClientId.Equals(clientId)).Select(x => new MetadataDTO { Id = x.Id });
            var ctr = _dbContext.Contracts.AsNoTracking().Where(t => t.ClientId.Equals(clientId)).Select(x => new ContractDTO { Id = x.Id });
            var files = _dbContext.Files.AsNoTracking().Select(x => new DocFile { MetaId = x.MetaId, NonFormDocId = x.NonFormDocId, ContractId = x.ContractId, FileSize = x.FileSize });
            var sumMeta = (from d in meta
                           join f in files
                          on d.Id equals f.MetaId
                          into FileTable
                           from x in FileTable.DefaultIfEmpty()
                           select x.FileSize).Sum();
            var sumCtr = (from d in ctr
                          join f in files
                         on d.Id equals f.ContractId
                         into FileTable
                          from x in FileTable.DefaultIfEmpty()
                          select x.FileSize).Sum();

            var sumSizeFile = sumCtr + sumMeta;

            return sumSizeFile.GetValueOrDefault();
        }

        public List<FileBO> GetListContractsFilesFor(int clientId, int take, int skip)
        {
            var c = (from contracts in _dbContext.Contracts.Include(t => t.DocKind).Include(t => t.DocType).Include(t => t.Organization).Include(t => t.Contractor)
                     join file in _dbContext.Files
                                           on contracts.Id equals file.ContractId
                     where contracts.ClientId == clientId && file.FileName != null

                     select new FileBO
                     {
                         Id = (int)file.Id,
                         FileName = file.FileName,
                         FileSize = file.FileSize.GetValueOrDefault(),
                         DocDate = contracts.DocDate,
                         Amount = contracts.Amount.GetValueOrDefault(),
                         DocNumber = contracts.DocNumber,
                         DocKind = contracts.DocKind == null ? "" : contracts.DocKind.Name,
                         DocType = contracts.DocType == null ? "" : contracts.DocType.Name,
                         Organization = contracts.Organization == null ? "" : contracts.Organization.Name,
                         Contractor = contracts.Contractor == null ? "" : contracts.Contractor.Name,
                         DocNumTaxInvoice = "",
                         // DocDateTaxInvoice = DateTime.Today
                     }).Skip(skip).Take(take).ToList();

            return c;
        }

        public List<FileBO> GetListMetadatasFilesFor(int clientId, int take, int skip)
        {

            var m = (from metadata in _dbContext.Metadatas.Include(t => t.DocKind).Include(t => t.DocType).Include(t => t.Organization).Include(t => t.Contractor)
                     join file in _dbContext.Files
                     on metadata.Id equals file.MetaId

                     where metadata.ClientId == clientId && file.FileName != null
                     select new FileBO
                     {
                         Id = (int)file.Id,
                         FileName = file.FileName,
                         FileSize = file.FileSize.GetValueOrDefault(),
                         DocDate = metadata.DocDate,
                         Amount = metadata.Amount.GetValueOrDefault(),
                         DocNumber = metadata.DocNumber,
                         DocKind = metadata.DocKind == null ? "" : metadata.DocKind.Name,
                         DocType = metadata.DocType == null ? "" : metadata.DocType.Name,
                         Organization = metadata.Organization == null ? "" : metadata.Organization.Name,
                         Contractor = metadata.Contractor == null ? "" : metadata.Contractor.Name,
                         DocNumTaxInvoice = metadata.DocNumTaxInvoice,
                         //  DocDateTaxInvoice = metadata.DocDateTaxInvoice
                     }).Skip(skip).Take(take).ToList();
            return m;

        }

        public List<FileBO> GetListNonFormFilesFor(int clientId, int take, int skip)
        {
            var m = (from nonformdocs in _dbContext.NonFormDocs
                     join file in _dbContext.Files
                      on nonformdocs.Id equals file.NonFormDocId
                     where nonformdocs.ClientId == clientId && file.FileName != null

                     select new FileBO
                     {
                         Id = (int)file.Id,
                         FileName = file.FileName,
                         FileSize = file.FileSize.GetValueOrDefault(),
                         DocDate = nonformdocs.Created


                     }).Skip(skip).Take(take).ToList();
            return m;
        }

        public List<FileBO> GetListAllFilesFor(int clientId, int take, int skip)
        {
            var all = (from metadata in _dbContext.Metadatas.Include(t => t.DocKind).Include(t => t.DocType).Include(t => t.Organization).Include(t => t.Contractor)
                       join file in _dbContext.Files
                       on metadata.Id equals file.MetaId

                       where metadata.ClientId == clientId && file.FileName != null && metadata.Deleted != true
                       select new FileBO
                       {
                           Id = (int)file.Id,
                           FileName = file.FileName,
                           FileSize = file.FileSize.GetValueOrDefault(),
                           DocDate = metadata.DocDate,
                           Amount = metadata.Amount,
                           DocNumber = Convert.ToString(metadata.DocNumber),
                           DocKind = Convert.ToString(metadata.DocKind.Name),
                           DocType = Convert.ToString(metadata.DocType.Name),
                           Organization = Convert.ToString(metadata.Organization.Name),
                           Contractor = Convert.ToString(metadata.Contractor.Name),
                           DocNumTaxInvoice = Convert.ToString(metadata.DocNumTaxInvoice),
                           DocDateTaxInvoice = metadata.DocDateTaxInvoice
                       }).Union((from contracts in _dbContext.Contracts.Include(t => t.DocKind).Include(t => t.DocType).Include(t => t.Organization).Include(t => t.Contractor)
                                 join file in _dbContext.Files
                                                       on contracts.Id equals file.ContractId
                                 where contracts.ClientId == clientId && file.FileName != null && contracts.Deleted != true

                                 select new FileBO
                                 {
                                     Id = (int)file.Id,
                                     FileName = file.FileName,
                                     FileSize = file.FileSize.GetValueOrDefault(),
                                     DocDate = contracts.DocDate,
                                     Amount = contracts.Amount,
                                     DocNumber = Convert.ToString(contracts.DocNumber),
                                     DocKind = Convert.ToString(contracts.DocKind.Name),
                                     DocType = Convert.ToString(contracts.DocType.Name),
                                     Organization = Convert.ToString(contracts.Organization.Name),
                                     Contractor = Convert.ToString(contracts.Contractor.Name),
                                     DocNumTaxInvoice = Convert.ToString(""),
                                     DocDateTaxInvoice = null

                                 })).Skip(skip).Take(take).ToList();
            return all;
        }
    }
}
