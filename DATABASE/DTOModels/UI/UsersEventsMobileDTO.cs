using System;
using System.Collections.Generic;
using System.Text;

namespace ARCHIVE.COMMON.DTOModels.UI
{
    public class UsersEventsMobileDTO : UsersEventsDTO
    {
        public string IdKey { get; set; }
        public string DocType { get; set; }
        public string DocNumber { get; set; }
        public DateTime? DocDate { get; set; }
    }
}
