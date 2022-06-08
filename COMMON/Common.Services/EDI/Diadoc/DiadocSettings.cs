using ARCHIVE.COMMON.Entities;
using Diadoc.Api;

namespace CloudArchive.Services.EDI.EnsolDiadoc
{
    public class DiadocSettings : IEDISettings
    {
        public EDISettings ConnectionInfo { get; set; }
        public DiadocApi Connection { get; set; }
        public string DiadocApiClientID { get; set; }
        public string Token { get; set; }


        public DiadocSettings(EDISettings connectionInfo, string ApiClientID)
        {
            DiadocApiClientID = ApiClientID;
            ConnectionInfo = connectionInfo;
        }
    }
}
