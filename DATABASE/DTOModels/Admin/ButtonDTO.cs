using System;
using System.Collections.Generic;
using System.Text;

namespace ARCHIVE.COMMON.DTOModels.Admin
{
    public class ButtonDTO
    {
        public ButtonDTO(int id)
        {
            Id = id;
        }

        public ButtonDTO(string userId)
        {
            UserId = userId;
        }

        public ButtonDTO(int id, int clientId)
        {
            Id = id;
            ClientId = clientId;
        }

        public ButtonDTO(int id, int clientId, int organizationId)
        {
            Id = id;
            ClientId = clientId;
            OrganizationId = organizationId;
        }

        public int Id { get; set; }
        public int ClientId { get; set; }
        public int OrganizationId { get; set; }
        public string UserId { get; set; }
        public string ItemId
        {
            get
            {
                return Id > 0 ? Id.ToString() : UserId;
            }
        }
    }
}
