using ARCHIVE.COMMON.DTOModels.Admin;
using ARCHIVE.COMMON.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ARCHIVE.COMMON.DTOModels
{
    public class SubstitutionDTO
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string User { get; set; }
        public string Substitute { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Comment { get; set; }
    }
}
