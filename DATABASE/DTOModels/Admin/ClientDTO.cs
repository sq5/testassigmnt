using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ARCHIVE.COMMON.DTOModels.Admin
{
    public class ClientDTO
    {
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; }
        public string Token { get; set; }
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
        public string LegalName { get; set; }
        public string Email { get; set; }
        public DateTime? BlockDate { get; set; }
        public int? TariffId { get; set; }
        public bool? UnicPerms { get; set; }
        public DateTime Created { get; set; }
        public string Phone { get; set; }
        public DateTime? LastLogin { get; set; }
        public int? OCRQuota { get; set; }
        public int? OCRUsed { get; set; }
    }
}
