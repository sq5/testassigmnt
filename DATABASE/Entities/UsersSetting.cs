using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ARCHIVE.COMMON.Entities
{
    public class UsersSetting
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string User { get; set; }
        [Required, MaxLength(100)]
        public string SettName { get; set; }
        [Required, MaxLength(1000)]
        public string SettValue { get; set; }
    }
}
