using System;
using System.Collections.Generic;
using System.Text;

namespace ARCHIVE.COMMON.Admin
{
    public class Role
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public List<string> UserIds { get; set; }
    }
}
