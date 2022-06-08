using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARCHIVE.COMMON.Entities
{
    public class ReestrPerms
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required, MaxLength(30)]
        public string DeniedReestr { get; set; }
        [Required, MaxLength(100)]
        public string User { get; set; }
    }
}

