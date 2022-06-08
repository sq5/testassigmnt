using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ARCHIVE.COMMON.Entities
{
     public class  BackgroundServiceLog
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        
        [ MaxLength(100)]
        public string Type { get; set; }
        [Required]
        public int Client { get; set; }
        public DateTime Time { get; set; }
        public string Message { get; set; }
        public string Service { get; set; }

    }
}
