using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARCHIVE.COMMON.Entities
{
    public class SignaturesAndEDIEvents
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Signer { get; set; }
        public Int64 FileID { get; set; }
        [Required]
        public DateTime EventDate { get; set; }
        [Required]
        [MaxLength(50)]
        public string System { get; set; }
        public byte[] SignatureBin { get; set; }
        public bool Approved { get; set; }
        [MaxLength(500)]
        public string Comment { get; set; }
        public Int64 MetaID { get; set; }
        public int ContractID { get; set; }
        [MaxLength(500)]
        public string Event { get; set; }
    }
}
