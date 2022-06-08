using ARCHIVE.COMMON.Entities;
using System.Collections.Generic;

namespace CloudArchive.Services.EDI
{
    public interface IClientContractWorker
    {
        List<Contract> Documents { get; set; }
        void FindNextBatch();
    }
}