using ARCHIVE.COMMON.DTOModels.Admin;
using ARCHIVE.COMMON.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ARCHIVE.COMMON.DTOModels
{
    public class ClientsTasksDTO
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public bool Active { get; set; }
        public string Task { get; set; }
        public string State { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? ClientId { get; set; }
        public ClientDTO Client { get; set; }
        public string Log { get; set; }

        public ClientsTasksDTO ActivateTask()
        {
            this.State = "В процессе";
            this.Active = true;
            this.StartDate = DateTime.Today;

            var clone = (ClientsTasksDTO)this.MemberwiseClone();
            clone.Client = null;
            return clone;
        }
        public ClientsTasksDTO DeactivateTask(string state, string log = "")
        {
            this.Active = false;
            this.EndDate = DateTime.Now;
            this.Log = log;
            this.State = state;

            var clone = (ClientsTasksDTO)this.MemberwiseClone();
            clone.Client = null;
            return clone;
        }
    }

}
