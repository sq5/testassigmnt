using System;
using System.Collections.Generic;
using System.Text;
using ARCHIVE.COMMON.Entities;

namespace ARCHIVE.COMMON.DTOModels
{
    public class WFTemplateDTO
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int DocTypeID { get; set; }
        public string Title { get; set; }
        public List<UsersTasksDTO> WF { get; set; }
    }
}
