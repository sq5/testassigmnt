using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARCHIVE.COMMON.Entities
{
    public class UsersTasks
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public bool Active { get; set; }
        [Required, MaxLength(500)]
        public string Users { get; set; }
        [MaxLength(500)]
        public string Initiator { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? DeadLine { get; set; }
        [Required]
        public DateTime Created { get; set; }
        public long? MetadataId { get; set; }
        public Metadata Metadata { get; set; }
        public int? ContractId { get; set; }
        public Contract Contract { get; set; }
        [MaxLength(500)]
        public string Comment { get; set; }
        [MaxLength(20)]
        public string Resolution { get; set; }
        [Required, MaxLength(20)]
        public string TaskType { get; set; }
        [Required]
        public int Stage { get; set; }
        [Required]
        public int Order { get; set; }
        [MaxLength(500)]
        public string TaskText { get; set; }
        [MaxLength(20)]
        public string ApprovementType { get; set; }

        [MaxLength(255)]
        public string SubstituteFor { get; set; }
        
    }
}
