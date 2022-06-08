using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ARCHIVE.COMMON.Entities
{
    public class UserClient
    {
        public string UserId { get; set; }
        public AppUser User { get; set; }
        [Required]
        public int? ClientId { get; set; }
        public Client Client { get; set; }
    }
}
