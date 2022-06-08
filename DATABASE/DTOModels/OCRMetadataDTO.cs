using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ARCHIVE.COMMON.DTOModels.UI;

namespace ARCHIVE.COMMON.Entities
{
    public class OCRMetadataDTO : MetadataDTO
    {
        public string OCRtype { get; set; }
        public string ContractorINN { get; set; }
        public string ContractorKPP { get; set; }
        public string OrganizationINN { get; set; }
        public string OrganizationKPP { get; set; }
        public string ContractorName { get; set; }
        public string OrganizationName { get; set; }
        public string AmountTotal { get; set; }
    }
}
