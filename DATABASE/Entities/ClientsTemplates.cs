using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARCHIVE.COMMON.Entities
{
    public class ClientsTemplates
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string TemplateName { get; set; }
        [Required, MaxLength(100)]
        public string FileName { get; set; }
        [Required]
        public byte[] FileBin { get; set; }
        [Required]
        public int FileSize { get; set; }
        [Required]
        public int DocTypeId { get; set; }
        [Required]
        public int ClientId { get; set; }
        public Client Client { get; set; }
        [MaxLength(200)]
        public string Comment { get; set; }
    }
}
