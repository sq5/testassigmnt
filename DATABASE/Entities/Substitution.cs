using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ARCHIVE.COMMON.Entities
{
    public class Substitution
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ClientId { get; set; }
        [Required]
        [MaxLength(255)]
        public string User { get; set; }
        [Required]
        [MaxLength(255)]
        public string Substitute { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [MaxLength(200)]
        public string Comment { get; set; }
    }
}
