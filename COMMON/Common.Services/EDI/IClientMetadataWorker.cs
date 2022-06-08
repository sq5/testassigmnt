using ARCHIVE.COMMON.Entities;
using System.Collections.Generic;

namespace CloudArchive.Services.EDI
{
    public interface IClientMetadataWorker
    {
        List<Metadata> Documents { get; set; }
        void FindNextBatch();
    }
}