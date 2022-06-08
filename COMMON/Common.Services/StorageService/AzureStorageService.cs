// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using ARCHIVE.COMMON.DTOModels;
using ARCHIVE.COMMON.Entities;
using ARCHIVE.COMMON.Extensions;
using ARCHIVE.COMMON.Servises;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using COMMON.Models;
using DATABASE.Context;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;

namespace COMMON.Common.Services.StorageService
{
    public class AzureStorageService<T> : IStorageService<T> where T : StoredFile
    {
        private readonly IConfiguration _configuration;
        private Lazy<BlobContainerClient> _blobContainerClient;
        private readonly SearchServiceDBContext _searchServiceDBContext;
        private readonly IAdminService _DataBase;

        public AzureStorageService(IConfiguration configuration, SearchServiceDBContext searchServiceDBContext, IAdminService DataBase)
        {
            _configuration = configuration;
            _searchServiceDBContext = searchServiceDBContext;
            _DataBase = DataBase;
            var cs = _configuration["AzureStorageConnectionString"];
            var cn = _configuration["AzureStorageDefaultContainerName"];
            _blobContainerClient = new Lazy<BlobContainerClient>(() => { return new BlobContainerClient(cs, cn); });
        }

        public async Task<byte[]> GetFileAsync(DocFile file)
        {
            if (file == null)
                return null;
            string URL = file.BlobUrl;
            if (string.IsNullOrEmpty(file.BlobUrl))
                URL = GetUrl(file.Id);
            BlobClient blob = _blobContainerClient.Value.GetBlobClient(URL);
            var result = await blob.DownloadContentAsync();
            return result.Value.Content.ToStream().ToByteArray();
        }

        public async Task<byte[]> GetFileAsync(BinariesDTO file)
        {
            string URL = file.BlobUrl;
            if (string.IsNullOrEmpty(file.BlobUrl))
                URL = GetUrl(file.Id);
            BlobClient blob = _blobContainerClient.Value.GetBlobClient(URL);
            var result = await blob.DownloadContentAsync();
            return result.Value.Content.ToStream().ToByteArray();
        }

        public async Task<byte[]> GetFileByIdAsync(long Id)
        {
            BlobClient blob = _blobContainerClient.Value.GetBlobClient(GetUrl(Id));
            var result = await blob.DownloadContentAsync();
            return result.Value.Content.ToStream().ToByteArray();
        }

        public async Task<bool> UploadFileAsync(long Id, byte[] file)
        {
            BlobClient blob = _blobContainerClient.Value.GetBlobClient(GetUrl(Id));
            Response<BlobContentInfo> response;
            using (MemoryStream ms = new MemoryStream(file))
            {
                response = await blob.UploadAsync(ms, true);
            }
            return true;
        }

        public async Task<bool> UploadFileAsync(string URL, byte[] file)
        {
            BlobClient blob = _blobContainerClient.Value.GetBlobClient(URL);
            Response<BlobContentInfo> response;
            using (MemoryStream ms = new MemoryStream(file))
            {
                response = await blob.UploadAsync(ms, true);
            }
            return true;
        }

        public async Task<Dictionary<long, byte[]>> GetFilesByIdAsync(IEnumerable<long> Ids)
        {
            RefReshStorage();
            Dictionary<long, byte[]> results = new Dictionary<long, byte[]>();
            foreach (var id in Ids.Distinct())
            {
                BlobClient blob = _blobContainerClient.Value.GetBlobClient(GetUrl(id));
                var result = await blob.DownloadContentAsync();
                results.Add(id, result.Value.Content.ToStream().ToByteArray());
            }
            return results;
        }


        public async Task<bool> AttachBinary(DocFile file)
        {
            try
            {
                BlobClient blob = _blobContainerClient.Value.GetBlobClient(GetUrl(file.Id));
                var result = await blob.DownloadContentAsync();
                file.FileBin = result.Value.Content.ToStream().ToByteArray();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CreateOrUpdateAsync(DocFile file)
        {
            if (file == null) throw new NullReferenceException(nameof(file));
            var isNew = file.Id < 1;
            var data = file.FileBin;
            file.FileBin = null;
            if (isNew)
            {
                _searchServiceDBContext.Files.Add(file);
                var c = _searchServiceDBContext.SaveChanges();
            }
            if (string.IsNullOrEmpty(file.BlobUrl))
                file.BlobUrl = GetUrl(file);
            var res = await UploadFileAsync(file.BlobUrl, data);
            if (res)
            {
                var c = _searchServiceDBContext.SaveChanges();
                if (c == 1)
                    return true;
            }
            return false;
        }

        public async Task<bool> CreateOrUpdateAsync(BinariesDTO file)
        {
            if (file == null) throw new NullReferenceException(nameof(file));
            var isNew = file.Id < 1;
            var data = Convert.FromBase64String(file.FileBase64);
            file.FileBase64 = null;
            if (isNew)
            {
                var c = await _DataBase.CreateAsyncInt64<BinariesDTO, DocFile>(file);
                file.Id = c;
            }
            if (string.IsNullOrEmpty(file.BlobUrl))
                file.BlobUrl = GetUrl(file);
            var res = await UploadFileAsync(file.BlobUrl, data);
            if (res)
            {
                var c = await _DataBase.UpdateAsync<BinariesDTO, DocFile>(file);
                return c;
            }
            return false;
        }

        public async Task<bool> DeleteAsync(BinariesDTO file)
        {
            if (file == null) throw new NullReferenceException(nameof(file));
            var res = await _blobContainerClient.Value.DeleteBlobIfExistsAsync(file.BlobUrl);
            if (res)
            {
                var success = await _DataBase.DeleteAsync<DocFile>(f => f.Id.Equals(file.Id));
                if (success.res)
                    return true;
            }
            return false;
        }

        public async Task<bool> DeleteAsync(DocFile file)
        {
            return await DeleteAsync(file, true);
        }


        public async Task<bool> DeleteAsync(DocFile file, bool SaveChanges = true)
        {
            if (file == null) throw new NullReferenceException(nameof(file));
            var res = await _blobContainerClient.Value.DeleteBlobIfExistsAsync(file.BlobUrl);
            if (res)
            {
                _searchServiceDBContext.Files.Remove(file);
                if (SaveChanges)
                {
                    var c = _searchServiceDBContext.SaveChanges();
                    if (c == 1)
                        return true;
                }
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteAsync(long Id)
        {
            return await DeleteAsync(Id, true);
        }

        public async Task<bool> DeleteAsync(long Id, bool SaveChanges = true)
        {
            DocFile file = _searchServiceDBContext.Files.Where(x => x.Id == Id).FirstOrDefault();
            var result = await DeleteAsync(file, SaveChanges);
            return result;
        }

        public async Task<bool> BatchDeleteAsync(List<long> Ids)
        {
            bool result = true;
            foreach (long id in Ids)
            {
                var res = await DeleteAsync(id, false);
                result = result && res;
            }
            result = _searchServiceDBContext.SaveChanges() == 1;
            return result;
        }

        public string GetUrl(long Id)
        {
            DocFile file = _searchServiceDBContext.Files.Where(x => x.Id == Id).FirstOrDefault();
            return GetUrl(file);
        }

        public string GetUrl(DocFile file)
        {
            if (!string.IsNullOrEmpty(file.BlobUrl))
                return file.BlobUrl;
            int ClientId = 0;
            if (file.MetaId > 0)
            {
                var doc = _searchServiceDBContext.Metadatas.Where(x => x.Id == file.MetaId).FirstOrDefault();
                if (doc.ClientId.HasValue)
                    ClientId = doc.ClientId.Value;
            }
            else if (file.ContractId > 0)
            {
                var doc = _searchServiceDBContext.Contracts.Where(x => x.Id == file.ContractId).FirstOrDefault();
                if (doc.ClientId.HasValue)
                    ClientId = doc.ClientId.Value;
            }
            else if (file.NonFormDocId > 0)
            {
                var doc = _searchServiceDBContext.NonFormDocs.Where(x => x.Id == file.NonFormDocId).FirstOrDefault();
                if (doc.ClientId.HasValue)
                    ClientId = doc.ClientId.Value;
            }
            var name = GenFileName(file.FileName);
            return ClientId + "/" + file.Id + "-" + name;
        }

        public string GetUrl(BinariesDTO file)
        {
            if (!string.IsNullOrEmpty(file.BlobUrl))
                return file.BlobUrl;
            int ClientId = 0;
            if (file.MetaId > 0)
            {
                var doc = _searchServiceDBContext.Metadatas.Where(x => x.Id == file.MetaId).FirstOrDefault();
                if (doc.ClientId.HasValue)
                    ClientId = doc.ClientId.Value;
            }
            else if (file.ContractId > 0)
            {
                var doc = _searchServiceDBContext.Contracts.Where(x => x.Id == file.ContractId).FirstOrDefault();
                if (doc.ClientId.HasValue)
                    ClientId = doc.ClientId.Value;
            }
            else if (file.NonFormDocId > 0)
            {
                var doc = _searchServiceDBContext.NonFormDocs.Where(x => x.Id == file.NonFormDocId).FirstOrDefault();
                if (doc.ClientId.HasValue)
                    ClientId = doc.ClientId.Value;
            }
            var name = GenFileName(file.FileName);
            return ClientId + "/" + file.Id + "-" + name;
        }

        private void RefReshStorage()
        {
            var cs = _configuration["AzureStorageConnectionString"];
            var cn = _configuration["AzureStorageDefaultContainerName"];
            _blobContainerClient = new Lazy<BlobContainerClient>(() => { return new BlobContainerClient(cs, cn); });
        }

        private string GenFileName(string filename)
        {
            filename = filename.Trim();
            filename = filename.Length == 0 ? "filename" : filename;
            filename = HttpUtility.UrlEncode(filename);
            filename = filename.Length > 500 ? filename.Substring(0, 499) : filename;
            return filename;
        }
    }
}
