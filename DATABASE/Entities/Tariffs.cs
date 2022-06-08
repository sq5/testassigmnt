using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ARCHIVE.COMMON.Entities
{
    public class Tariffs
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required, MaxLength(255)]
        public string Name { get; set; }
        public bool Archived { get; set; }
        public float Amount { get; set; }
        public string Period { get; set; }
        public int StorageQuota { get; set; }
    }
}
