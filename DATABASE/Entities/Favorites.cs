using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARCHIVE.COMMON.Entities
{
    public class Favorites
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [MaxLength(255)]
        [Required]
        public string UserName { get; set; }
        [Required]
        public string SettName { get; set; }
        [Required]
        public long MetaId { get; set; }
        [Required]
        public int ContractId { get; set; }
    }
}
