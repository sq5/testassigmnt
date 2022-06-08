using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ARCHIVE.COMMON.Entities
{
    public class Client
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; }
        [Required]
        public string Token { get; set; }
        [Required]
        public DateTime TokenExpires { get; set; }
        public float StorageQuota { get; set; }
        public float? StorageUsed { get; set; }
        public int UsersQuota { get; set; }
        public int? UsersUsed { get; set; }
        public bool? Blocked { get; set; }
        public string BillTo { get; set; }
        public string INN { get; set; }
        public string KPP { get; set; }
        public string BIK { get; set; }
        [MaxLength(200)]
        public string LegalName { get; set; }
        [MaxLength(200)]
        public string Email { get; set; }
        public DateTime? BlockDate { get; set; }
        public int? TariffId { get; set; }
        public Tariffs Tariff { get; set; }
        public bool? UnicPerms { get; set; }
        public DateTime Created { get; set; }
        public string Phone { get; set; }
        public DateTime? LastLogin { get; set; }
        public int? OCRQuota { get; set; }
        public int? OCRUsed { get; set; }
    }
}
