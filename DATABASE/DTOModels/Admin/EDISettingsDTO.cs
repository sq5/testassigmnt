using System;
using System.Collections.Generic;
using System.Text;

namespace ARCHIVE.COMMON.DTOModels.Admin
{
    public class EDISettingsDTO
    {       
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string EDILogin { get; set; }
        public string EDIPassword { get; set; }
        public string EDIProvider { get; set; }
        public string EDIUserID { get; set; }
        public string LastEvent { get; set; }
        public bool LoadContracts { get; set; }
        public bool LoadBuhDocs { get; set; }
        public DateTime? LastEventDate { get; set; }
        public string OrganizationKPP { get; set; }
        public string OrganizationINN { get; set; }
        public string OrganizationName { get; set; }
    }
}
