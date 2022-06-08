using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ARCHIVE.COMMON.DTOModels;
using ARCHIVE.COMMON.Entities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MySqlConnector.Logging;

namespace COMMON.Common.Services.StorageService
{
    public interface IStorageService<in T>
    {
        Task<byte[]> GetFileByIdAsync(long Id);
        Task<byte[]> GetFileAsync(DocFile file);
        Task<byte[]> GetFileAsync(BinariesDTO file);

        Task<Dictionary<long, byte[]>> GetFilesByIdAsync(IEnumerable<long> Id);

        Task<bool> UploadFileAsync(long Id, byte[] file);

        Task<bool> BatchDeleteAsync(List<long> Id);

        Task<bool> AttachBinary(DocFile file);

        Task<bool> CreateOrUpdateAsync(BinariesDTO file);
        Task<bool> CreateOrUpdateAsync(DocFile file);

        Task<bool> DeleteAsync(long Id);
        Task<bool> DeleteAsync(BinariesDTO file);
        Task<bool> DeleteAsync(DocFile file);

        string GetUrl(DocFile file);
        string GetUrl(long Id);
    }
}
