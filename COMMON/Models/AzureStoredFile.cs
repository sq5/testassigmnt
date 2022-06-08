using ARCHIVE.COMMON.Entities;
using COMMON.Common.Services.StorageService;

namespace COMMON.Models
{
    public class AzureStoredFile : StoredFile
    {
        public AzureStoredFile(DocFile docFile) : base(docFile)
        {

        }
        private byte[] fileBin;

        public override byte[] FileBin { get => fileBin; set => fileBin = value; }
    }
}
