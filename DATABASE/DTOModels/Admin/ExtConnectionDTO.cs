using System;
using System.Collections.Generic;
using System.Text;

namespace ARCHIVE.COMMON.DTOModels.Admin
{
    public class ExtConnectionDTO
    {
       
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string Type { get; set; }
        public string Server { get; set; }
        public int Port { get; set; }
        public bool TLS { get; set; }
        public string FolderProcessed { get; set; }
        public string FolderIn { get; set; }
        public string FolderError { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }
        public int? OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public bool? OCR { get; set; }
        public bool? OCRSplit { get; set; }
    }
}
