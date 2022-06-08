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
    public class DocumentMobileDTO
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
        public int Version1CExchange { get; set; }
        public string Comment { get; set; }
        public string DocKind { get; set; }
        public string DocType { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public string Reciever { get; set; }
        public string Address { get; set; }
        public string Delivery { get; set; }
        public string Contact { get; set; }
        public string Organization { get; set; }
        public string Contractor { get; set; }
        public string Contract { get; set; }
        public string Project { get; set; }
        public string State { get; set; }
        public string PaymentNumber { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public byte[] Picture { get; set; }
    }
}
