using System;
using System.Collections.Generic;
using System.Text;

namespace ARCHIVE.COMMON.DTOModels.Files
{
    public class FileBO
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string DocType { get; set; }
        public string DocKind { get; set; }
        public string DocNumber { get; set; }
        public DateTime? DocDate { get; set; }
        public double? Amount { get; set; }
        public string Organization { get; set; }
        public string Contractor { get; set; }
        public string DocNumTaxInvoice {get; set;}
        public DateTime? DocDateTaxInvoice { get; set; }
        public byte[] FileBin { get; set; }
        public int FileSize { get; set; }

    }
}
