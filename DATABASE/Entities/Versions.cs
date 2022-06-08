using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ARCHIVE.COMMON.Entities
{
    public class Versions
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public long? MetadataId { get; set; }
        public Metadata Metadata { get; set; }
        public int? ContractId { get; set; }
        public Contract Contract { get; set; }
        [Required,MaxLength(255)]
        public string Action { get; set; }
        public DateTime Date { get; set; }
        [Required,MaxLength(255)]
        public string User { get; set; }
    }
}
