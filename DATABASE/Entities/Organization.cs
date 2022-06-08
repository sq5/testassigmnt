using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ARCHIVE.COMMON.Entities
{
    public class Organization
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; }
        public string INN { get; set; }
        public string KPP { get; set; }
        [MaxLength(150)]
        public string Ext_ID { get; set; }
        [Required]
        public int? ClientId { get; set; }
        public Client Client { get; set; }
    }
}
