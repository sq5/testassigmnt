using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARCHIVE.COMMON.Entities
{
    public class ExtConnection
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required, MaxLength(100)]
        
        public int? ClientId { get; set; }
        public Client Client { get; set; }

        [MaxLength(100)]
        public string Type { get; set; }
        [Required, MaxLength(100)]
        public string Server { get; set; }
        public int Port { get; set; }
        [Required, MaxLength(100)]
        public bool TLS { get; set; }
        [Required, MaxLength(200)]
        public string FolderIn { get; set; }
        [Required, MaxLength(200)]
        public string FolderProcessed { get; set; }
        [Required, MaxLength(200)]
        public string FolderError { get; set; }
        public string User { get; set; }
        [Required, MaxLength(100)]
        public string Password { get; set; }
        [Required, MaxLength(50)]
        public bool IsActive { get; set; }
        public int? OrganizationId { get; set; }
        public bool? OCR { get; set; }
        public bool? OCRSplit { get; set; }
    }
}
