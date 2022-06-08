using System;
using System.Collections.Generic;
using System.Text;

namespace ARCHIVE.COMMON.DTOModels.Admin
{
    public class SignaturesAndEDIEventsDTO
    {
        public int Id { get; set; }
        public string Signer { get; set; }
        public Int64 FileID { get; set; }
        public DateTime EventDate { get; set; }
        public string SignatureBase64 { get; set; }
        public string System { get; set; }
        public bool Approved { get; set; }
        public string Comment { get; set; }
        public Int64 MetaID { get; set; }
        public int ContractID { get; set; }
        public string Event { get; set; }
        public string FileName { get; set; }
        public string DisplayName { get; set; }
        public string Position { get; set; }
        public byte[] Picture { get; set; }
    }
}
