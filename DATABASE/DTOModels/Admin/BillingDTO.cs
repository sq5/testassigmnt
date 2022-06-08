using System;
using System.Collections.Generic;
using System.Text;

namespace ARCHIVE.COMMON.DTOModels.Admin
{
    public class BillingDTO
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public DateTime? PaidTo { get; set; }
        public string Type { get; set; }
        public float Saldo { get; set; }
        public float Amount { get; set; }
        public bool FileExists { get; set; }
        public string Document { get; set; }
        public int? ClientId { get; set; }
        public string Client { get; set; }
        public bool? Paid { get; set; }
    }
}
