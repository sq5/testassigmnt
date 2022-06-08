using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ARCHIVE.COMMON.DTOModels.Admin
{
    public class ContractorDTO
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        public string INN { get; set; }
        public string KPP { get; set; }
        public int OrganizationId { get; set; }
        public string Organization { get; set; }
        public string Ext_ID { get; set; }
        public ButtonDTO ButtonDTO { get { return new ButtonDTO(Id); } }
    }
}
