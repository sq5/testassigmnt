using ARCHIVE.COMMON.Entities;

namespace CloudArchive.Services.EDI
{
    public interface IEDISettings
    {
        EDISettings ConnectionInfo { get; set; }
    }
}
