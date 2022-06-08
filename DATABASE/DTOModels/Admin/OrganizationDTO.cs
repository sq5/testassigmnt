using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ARCHIVE.COMMON.DTOModels.Admin
{
    public class OrganizationDTO
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        public string INN { get; set; }
        public string KPP { get; set; }
        [Required]
        [Display(Name = "Available clients")]
        public int ClientId { get; set; }
        public string Client { get; set; }
        public string Ext_ID { get; set; }
        public ButtonDTO ButtonDTO { get { return new ButtonDTO(Id); } }
    }
}
