using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARCHIVE.COMMON.Entities
{
    public class NonFormDocs
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime Modified { get; set; }
        public int? OrganizationId { get; set; }
        public DateTime Created { get; set; }
        [Required]
        public int? ClientId { get; set; }
        public Client Client { get; set; }
        public bool? Deleted { get; set; }
        public DateTime? DeleteDate { get; set; }
        [MaxLength(200)]
        public string Sender { get; set; }
        [MaxLength(255)]
        public string DeletedBy { get; set; }
        public bool? Declined { get; set; }
        [MaxLength(100)]
        public string DeclinedBy { get; set; }
        [MaxLength(500)]
        public string Comment { get; set; }
        public Organization Organization { get; set; }
        [MaxLength(100)]
        public string OCRState { get; set; }
        public string OCRXML { get; set; }
        public string OCRVerified { get; set; }
        public int? DocTypeId { get; set; }
        public DocType DocType { get; set; }
        public bool? OCRSplit { get; set; }
        public DateTime? DocDate { get; set; }
        public int? ContractorId { get; set; }
        public Contractor Contractor { get; set; }
        public double? Amount { get; set; }
    }
}
