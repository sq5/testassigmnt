using System;
using System.Collections.Generic;
using System.Text;

namespace ARCHIVE.COMMON.DTOModels.Admin
{
    public class VersionDTO
    {
        public int Id { get; set; }
        public long? MetadataId { get; set; }
        public int? ContractId { get; set; }
        public string Action { get; set; }
        public DateTime Date { get; set; }
        public string User { get; set; }
        public byte[] Picture { get; set; }
        public string Position { get; set; }
    }
}
