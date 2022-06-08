using System;
using System.Collections.Generic;
using System.Text;

namespace ARCHIVE.COMMON.DTOModels.Admin
{
    public class BackgroundServiceLogDTO
    {
      
        public int Id { get; set; }

        public string Type { get; set; }
       
        public int Client { get; set; }
        public DateTime Time { get; set; }

        public string Message { get; set; }

        public string Service { get; set; }

    }
}
