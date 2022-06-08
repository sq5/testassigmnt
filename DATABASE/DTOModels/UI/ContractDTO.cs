using Newtonsoft.Json;
using ARCHIVE.COMMON.DTOModels;
using ARCHIVE.COMMON.DTOModels.Admin;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ARCHIVE.COMMON.Entities
{
    public class ContractDTO
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
        [MaxLength(150)]
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
        public DocKind DocKind { get; set; }
        public string DocKindName { get; set; }
        public int? DocTypeId { get; set; }
        public string DocType { get; set; }
        public string State { get; set; }
        public ContractorDTO Contractor { get; set; }
        public OrganizationDTO Organization { get; set; }
        public Project Project { get; set; }
        public int ClientId { get; set; }
        public string Client { get; set; }
        public ICollection<BinariesDTO> Binaries { get; set; }
        public bool Deleted { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string EDIId { get; set; }
        public string EDIState { get; set; }
        public string EDIProvider { get; set; }
        public bool EDIProcessed { get; set; }
        public bool EDIIsIncoming { get; set; }
        public bool EDILocalSigned { get; set; }
        public bool EdiNeedExport { get; set; }
    }
}
