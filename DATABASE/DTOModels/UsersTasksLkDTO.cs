using System;
using System.Collections.Generic;
using System.Text;

namespace ARCHIVE.COMMON.DTOModels
{
    public class UsersTasksLkDTO
    {
        public long Id { get; set; }
        public string IdKey { get; set; }
        public string SettName { get; set; }
        public string DocType { get; set; }
        public string DocKind { get; set; }
        public string DocNumber { get; set; }
        public DateTime? DocDate { get; set; }
        public double? Amount { get; set; }
        public double? AmountToPay { get; set; }
        public string ContractorName { get; set; }
        public string OrganizationName { get; set; }
        public string ProjectName { get; set; }
        public string ContractName { get; set; }
        public DateTime? DeadLine { get; set; }
        public DateTime? DocModified { get; set; }
        public DateTime Created { get; set; }
        public string Executor { get; set; }
        public long ParId { get; set; }
        public string Comment { get; set; }
        public string TaskText { get; set; }
        public string DisplayName { get; set; }
        public string UserName { get; set; }
        public string State { get; set; }
        public string Position { get; set; }
        public bool Active { get; set; }
        public string Resolution { get; set; }
        public byte[] Picture { get; set; }
        public bool FileExists { get; set; }
        public string EDIProvider { get; set; }
        public string Ext_ID { get; set; }
        public bool Signed { get; set; }
        public bool Paid { get; set; }
        public string SubstituteFor { get; set; }

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
