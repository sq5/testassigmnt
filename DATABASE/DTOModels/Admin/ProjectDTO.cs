using System;
using System.Collections.Generic;
using System.Text;

namespace ARCHIVE.COMMON.DTOModels.Admin
{
    public class ProjectDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ProjectLeader { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string State { get; set; }
        public string Comment { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
    }
}
