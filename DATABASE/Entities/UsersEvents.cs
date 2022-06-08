using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ARCHIVE.COMMON.Entities
{
    public class UsersEvents
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string User { get; set; }
        [Required]
        public DateTime EventDate { get; set; }
        public string EventText { get; set; }
        public Int64 MetaID { get; set; }
        public int ContractID { get; set; }
        public int NonFormDocId { get; set; }
    }
}
