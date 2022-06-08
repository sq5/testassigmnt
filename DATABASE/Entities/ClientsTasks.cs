using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARCHIVE.COMMON.Entities
{
    public class ClientsTasks
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public DateTime Created { get; set; }
        [Required]
        public bool Active { get; set; }
        [Required, MaxLength(50)]
        public string Task { get; set; }
        [MaxLength(50)]
        public string State { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [Required]
        public int? ClientId { get; set; }
        public Client Client { get; set; }
        [MaxLength(2000)]
        public string Log { get; set; }
    }
}
