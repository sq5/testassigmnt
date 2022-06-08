using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Text;

namespace ARCHIVE.COMMON.Entities
{
    public class AppUser: IdentityUser
    {
        [NotMapped]
        public IList<Claim> Claims { get; set; } = new List<Claim>();
        public bool Blocked { get; set; }
        public string DisplayName { get; set; }
        [MaxLength(50)]
        public string Position { get; set; }
        public byte[] Picture { get; set; }
        [MaxLength(16)]
        public string Phone { get; set; }
        [MaxLength(16)]
        public string ExtPhone { get; set; }
        public bool ResetPassword { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}
