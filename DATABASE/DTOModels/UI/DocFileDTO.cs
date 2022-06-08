using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ARCHIVE.COMMON.Entities
{
    public class DocFileDTO
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int64 Id { get; set; }
        [Required]
        public Int64 MetaId { get; set; }
        public int ContractId { get; set; }
        public int? NonFormDocId { get; set; }
        [Required, MaxLength(255)]
        public string FileName { get; set; }
        public int? FileSize { get; set; }
        [Required]
        public byte[] FileBin { get; set; }
        [Required]
        public DateTime Created { get; set; }
        [Required]
        public DateTime Modified { get; set; }
        public bool HasSignature { get; set; }
    }
}
