using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ARCHIVE.COMMON.Entities
{
    public class WFTemplates
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [MaxLength(200)]
        public string Title { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; }
        public int DocTypeID { get; set; }
        [Required]
        public byte[] WF { get; set; }
    }
}
