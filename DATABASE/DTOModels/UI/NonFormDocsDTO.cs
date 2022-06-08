using ARCHIVE.COMMON.DTOModels.Admin;
using System;
using System.Collections.Generic;
using System.Text;

namespace ARCHIVE.COMMON.DTOModels.UI
{
    public class NonFormDocsDTO
    {
        public int? DocTypeId { get; set; }
        public string DocType { get; set; }
        public int Id { get; set; }
        public string RequestID { get; set; }
        public int? OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Created { get; set; }
        public int ClientId { get; set; }
        public string Client { get; set; }
        public bool? Deleted { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string Sender { get; set; }
        public bool? Declined { get; set; }
        public string DeclinedBy { get; set; }
        public string Comment { get; set; }
        public bool FileExists { get; set; }
        public string FileName { get; set; }
        public int? FileSize { get; set; }
        public OrganizationDTO Organization { get; set; }
        public ICollection<BinariesDTO> Binaries { get; set; }
        public string OCRState { get; set; }
        public string OCRVerified { get; set; }
        public bool? OCRSplit { get; set; }
        public DateTime? DocDate { get; set; }
        public int? ContractorId { get; set; }
        public ContractorDTO Contractor { get; set; }
        public string ContractorName { get; set; }
        public double? Amount { get; set; }
    }
}
