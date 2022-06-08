using System;
using System.Collections.Generic;
using System.Text;

namespace ARCHIVE.COMMON.DTOModels
{
    public class DocApprovalDTO
    {
        public string RequestID { get; set; }
        public int NonFormID { get; set; }
        public bool Approved { get; set; }
        public string Approver { get; set; }
        public string Comment { get; set; }
    }
}
