using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARCHIVE.COMMON.Entities
{
    public class Billing
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public DateTime? PaidTo { get; set; }
        public float Saldo { get; set; }
        public float Amount { get; set; }
        [Required, MaxLength(20)]
        public string Type { get; set; }
        [Required]
        public byte[] Document { get; set; }
        [Required]
        public int? ClientId { get; set; }
        public Client Client { get; set; }
        public bool? Paid { get; set; }
    }
}
