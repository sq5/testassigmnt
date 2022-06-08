using ARCHIVE.COMMON.DTOModels.Admin;
using ARCHIVE.COMMON.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ARCHIVE.COMMON.DTOModels.UI
{
    public class MetadataClientDTO
    {
        public Int64 Id { get; set; }
        public string Source { get; set; }
        public string DocNumber { get; set; }
        public DateTime? DocDate { get; set; }
        public double? Amount { get; set; }
        public double? AmountWOVAT { get; set; }
        public double? AmountToPay { get; set; }
        public double? VAT { get; set; }
        public string Currency { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Modified { get; set; }
        public string DocNumTaxInvoice { get; set; }
        public DateTime? DocDateTaxInvoice { get; set; }
        public string DocNumInvoice { get; set; }
        public DateTime? DocDateInvoice { get; set; }
        public bool UploadedTo1C { get; set; }
        public string ModifiedById { get; set; }
        public bool Signed { get; set; }
        public bool Paid { get; set; }
        public int ClientId { get; set; }
        public string RequestID { get; set; }
        public bool FileExists { get; set; }
        public int Version1CExchange { get; set; }
        public string ContractName { get; set; }
        public string Comment { get; set; }
        public string DocKind { get; set; }
        public string DocType { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public string Reciever { get; set; }
        public string Address { get; set; }
        public string Delivery { get; set; }
        public string Contact { get; set; }
        public Organization Organization { get; set; }
        public Contractor Contractor { get; set; }
        public Contract Contract { get; set; }
        public Project Project { get; set; }
        public string Ext_ID { get; set; }
        public string EDIId { get; set; }
        public string EDIState { get; set; }
        public string EDIProvider { get; set; }
        public bool EDIProcessed { get; set; }
        public string State { get; set; }
        public string PaymentNumber { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string String1 { get; set; }
        public string String2 { get; set; }
        public string String3 { get; set; }
        public string String4 { get; set; }
        public string String5 { get; set; }
        public string String6 { get; set; }
        public string String7 { get; set; }
        public string String8 { get; set; }
        public DateTime? Datetime1 { get; set; }
        public DateTime? Datetime2 { get; set; }
        public DateTime? Datetime3 { get; set; }
        public DateTime? Datetime4 { get; set; }
        public int? Int1 { get; set; }
        public int? Int2 { get; set; }
        public int? Int3 { get; set; }
        public int? Int4 { get; set; }
        public bool? Bool1 { get; set; }
        public bool? Bool2 { get; set; }
        public bool? Bool3 { get; set; }
        public bool? Bool4 { get; set; }
    }
}
