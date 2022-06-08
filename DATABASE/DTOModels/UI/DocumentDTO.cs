// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using ARCHIVE.COMMON.DTOModels;
using ARCHIVE.COMMON.Entities;

namespace DATABASE.DTOModels.UI
{
    public class DocumentDTO
    {
        public int ContrId { get; set; }
        public int MetaId { get; set; }
        public int Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string DocNumber { get; set; }
        public DateTime? DocDate { get; set; }
        public DateTime? ValidityPeriod { get; set; }
        public string Ext_ID { get; set; }
        public int? ContractID { get; set; }
        public int? NonFormID { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Modified { get; set; }
        public string ModifiedBy { get; set; }
        public string RequestID { get; set; }
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string ContractorName { get; set; }
        public bool FileExists { get; set; }
        public int Version1CExchange { get; set; }
        public double? Amount { get; set; }
        public double? AmountWOVAT { get; set; }
        public double? VAT { get; set; }
        public string Currency { get; set; }
        public string Comment { get; set; }
        public string Subject { get; set; }
        public string DocKind { get; set; }
        public string DocKindName { get; set; }
        public int? DocTypeId { get; set; }
        public string DocType { get; set; }
        public string State { get; set; }
        public Project Project { get; set; }
        public int ClientId { get; set; }
        public string Client { get; set; }
        public ICollection<BinariesDTO> Binaries { get; set; }
        public bool Deleted { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string EDIId { get; set; }
        public string EDIState { get; set; }
        public string EDIProvider { get; set; }
        public bool EDIProcessed { get; set; }
        public bool EDIIsIncoming { get; set; }
        public bool EDILocalSigned { get; set; }
        public bool EdiNeedExport { get; set; }
        public string Source { get; set; }
        public double? AmountToPay { get; set; }
        public string DocNumTaxInvoice { get; set; }
        public DateTime? DocDateTaxInvoice { get; set; }
        public string DocNumInvoice { get; set; }
        public DateTime? DocDateInvoice { get; set; }
        public bool UploadedTo1C { get; set; }
        public string ModifiedById { get; set; }
        public bool Signed { get; set; }
        public bool Paid { get; set; }
        public string ContractName { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public string Reciever { get; set; }
        public string Address { get; set; }
        public string Delivery { get; set; }
        public string Contact { get; set; }
        public string PaymentNumber { get; set; }
        public DateTime? PaymentDate { get; set; }
        public Contractor Contractor { get; set; }
        public Contract Contract { get; set; }
        public Organization Organization { get; set; }
        public string Reestr { get; set; }
    }
}
