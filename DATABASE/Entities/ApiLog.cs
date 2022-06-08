using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ARCHIVE.COMMON.Entities
{
    public class ApiLog
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required, MaxLength(200)]
        public string RequestID { get; set; }
        [Required, MaxLength(100)]
        public string Service { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required, MaxLength(100)]
        public string State { get; set; }
        [Required]
        public string JSON { get; set; }
        public string Exception { get; set; }
        public int? ClientId { get; set; }
        public Client Client { get; set; }
    }
}
