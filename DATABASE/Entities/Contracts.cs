// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel.DataAnnotations;

namespace ARCHIVE.COMMON.Entities
{
    public class Contract
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string Type { get; set; }
        [Required, MaxLength(255)]
        public string Name { get; set; }
        [MaxLength(80)]
        public string DocNumber { get; set; }
        public DateTime? DocDate { get; set; }
        public DateTime? ValidityPeriod { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        [Required, MaxLength(255)]
        public string ModifiedBy { get; set; }
        [Required, MaxLength(255)]
        public string CreatedBy { get; set; }
        [MaxLength(255)]
        public string DeletedBy { get; set; }
        [MaxLength(150)]
        public string Ext_ID { get; set; }
        public double? Amount { get; set; }
        public double? AmountWOVAT { get; set; }
        public double? VAT { get; set; }
        [MaxLength(3)]
        public string Currency { get; set; }
        [MaxLength(2000)]
        public string Comment { get; set; }
        [MaxLength(500)]
        public string Subject { get; set; }
        public int? DocKindId { get; set; }
        public DocKind DocKind { get; set; }
        public int OrganizationId { get; set; }
        public Organization Organization { get; set; }
        public int DocTypeId { get; set; }
        public DocType DocType { get; set; }
        public int ContractorId { get; set; }
        public Contractor Contractor { get; set; }
        [Required]
        public int? ClientId { get; set; }
        public Client Client { get; set; }
        public int? ProjectId { get; set; }
        public Project Project { get; set; }
        public bool Deleted { get; set; }
        public DateTime? DeleteDate { get; set; }
        [MaxLength(50)]
        public string State { get; set; }
        [MaxLength(200)]
        public string EDIId { get; set; }
        [MaxLength(50)]
        public string EDIState { get; set; }
        [MaxLength(50)]
        public string EDIProvider { get; set; }
        public bool EDIProcessed { get; set; }
        public bool EdiNeedExport { get; set; }
        public bool EDILocalSigned { get; set; }
        public bool EDIIsIncoming { get; set; }
    }
}
