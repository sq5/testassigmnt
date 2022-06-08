using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ARCHIVE.COMMON.Entities
{
    public class AdditionalFieldsMapping
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; }
        public string FieldName { get; set; }
        public string FieldColumn { get; set; }
        public string FieldType { get; set; }
        public string FieldValues { get; set; }
        public string DocTypes { get; set; }
    }
}
