using ARCHIVE.COMMON.DTOModels.Admin;
using ARCHIVE.COMMON.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Text;

namespace ARCHIVE.COMMON.DTOModels.UI
{
    public class RecycleDTO
    {
        public Int64 Id { get; set; }
        public string IdCombined { get; set; }
        public string DocNumber { get; set; }
        public DateTime? DocDate { get; set; }
        public DateTime Created { get; set; }
        public DateTime? DeleteDate { get; set; }
        public int ClientId { get; set; }
        public string Organization { get; set; }
        public string DocType { get; set; }
        public string Table { get; set; }
        public string Contractor { get; set; }
        public string Sender { get; set; }
        public double? Amount { get; set; }
        public string DeletedBy { get; set; }
        public string Comment { get; set; }
        public string DisplayName { get; set; }
        public string Position { get; set; }
        public byte[] Picture { get; set; }
    }
}
