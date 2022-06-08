using ARCHIVE.COMMON.DTOModels.Admin;
using System;
using System.Collections.Generic;
using System.Text;

namespace ARCHIVE.COMMON.DTOModels.UI
{
    public class BinariesWithFieldsDTO : BinariesDTO
    {
        public string DocNumTaxInvoice { get; set; }
        public DateTime? DocDateTaxInvoice { get; set; }
        public string DocNumInvoice { get; set; }
        public DateTime? DocDateInvoice { get; set; }
        public double? AmountToPay { get; set; }
        public string DocTypeName { get; set; }
        public string DocNumber { get; set; }
        public string Currency { get; set; }
        public DateTime? DocDate { get; set; }
        public double? Amount { get; set; }
        public double? AmountWOVAT { get; set; }
        public double? VAT { get; set; }
        public string TablePart { get; set; }
        public int? DocTypeId { get; set; }
        public ContractorDTO Contractor { get; set; }
        public string OCRtype { get; set; }
        public string AmountTotal { get; set; }
    }
}
