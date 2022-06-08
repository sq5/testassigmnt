using System;
using System.Collections.Generic;
using System.Text;

namespace ARCHIVE.COMMON.DTOModels.UI
{
    public class UsersEventsDTO
    {
        public int Id { get; set; }
        public string User { get; set; }
        public DateTime EventDate { get; set; }
        public string EventText { get; set; }
        public Int64 MetaID { get; set; }
        public int ContractID { get; set; }
        public int NonFormDocId { get; set; }
    }
}
