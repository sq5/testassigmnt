// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ARCHIVE.COMMON.DTOModels.Admin;
using ARCHIVE.COMMON.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ARCHIVE.COMMON.DTOModels.UI
{
    public class MetadataDTO
    {
        public Int64 Id { get; set; }
        public string PackageID { get; set; }
        public string CardData { get; set; }
        public string TablePart { get; set; }
        [MaxLength(30)]
        public string Source { get; set; }
        [MaxLength(80)]
        public string DocNumber { get; set; }
        public DateTime? DocDate { get; set; }
        public double? Amount { get; set; }
        public double? AmountWOVAT { get; set; }
        public double? AmountToPay { get; set; }
        public double? VAT { get; set; }
        public string Currency { get; set; }
        public long? MetaID { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public string DocNumTaxInvoice { get; set; }
        public DateTime? DocDateTaxInvoice { get; set; }
        public string DocNumInvoice { get; set; }
        public DateTime? DocDateInvoice { get; set; }
        public int? NonFormID { get; set; }
        public bool UploadedTo1C { get; set; }
        public string ModifiedBy { get; set; }
        public string CreatedBy { get; set; }
        public bool Signed { get; set; }
        public bool Paid { get; set; }
        public int ClientId { get; set; }
        public string Client { get; set; }
        public int DocTypeId { get; set; }
        public string DocType { get; set; }
        public string Ext_ID { get; set; }
        public string RequestID { get; set; }
        public bool FileExists { get; set; }
        public string Comment { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public string Reciever { get; set; }
        public string Address { get; set; }
        public string Delivery { get; set; }
        public string Contact { get; set; }
        public string State { get; set; }
        public int Version1CExchange { get; set; }
        public string NotifyUser { get; set; }
        public string Operation { get; set; }
        public OrganizationDTO Organization { get; set; }
        public DocKind DocKind { get; set; }
        public int? DocKindId { get; set; }
        public ContractorDTO Contractor { get; set; }
        public ContractDTO Contract { get; set; }
        public Project Project { get; set; }
        public ICollection<BinariesDTO> Binaries { get; set; }
        public bool Deleted { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string EDIId { get; set; }
        [MaxLength(50)]
        public string EDIState { get; set; }
        [MaxLength(50)]
        public string EDIProvider { get; set; }
        public bool EDIProcessed { get; set; }
        public bool EDIIsIncoming { get; set; }
        public bool EDILocalSigned { get; set; }
        public bool EdiNeedExport { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string PaymentNumber { get; set; }
    }
}
