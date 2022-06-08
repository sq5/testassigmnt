using System;
using System.Collections.Generic;
using System.Text;

namespace ARCHIVE.COMMON.DTOModels.Admin
{
    public class ApiLogDTO
    {
        public int Id { get; set; }
        public string RequestID { get; set; }
        public string Service { get; set; }
        public DateTime Date { get; set; }
        public string State { get; set; }

        public string JSON { get; set; }
        public string Exception { get; set; }
        public int ClientId { get; set; }
    }
}
