using ARCHIVE.COMMON.DTOModels.Admin;
using ARCHIVE.COMMON.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ARCHIVE.COMMON.DTOModels
{
    public class UserDTOWithPicture
    {
        public string Id { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string Password { get; set; }
        public string Confirm { get; set; }
        public List<string> Roles { get; set; }
        public bool Blocked { get; set; }
        public bool ClientAdmin { get; set; }
        public bool EnDocsAdmin { get; set; }
        public bool Demo { get; set; }
        public byte[] Picture { get; set; }
        public string Position { get; set; }
        public string Phone { get; set; }
        public string ExtPhone { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public Client Client { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}
