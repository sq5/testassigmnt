using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARCHIVE.COMMON.Entities
{
    public class EDISettings
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ClientID { get; set; }
       ///public int OrganizationId { get; set; }
        [Required, MaxLength(300)]
        public string EDILogin { get; set; }
        [MaxLength(300)]
        public string EDIPassword { get; set; }
        [Required, MaxLength(50)]
        public string EDIProvider { get; set; }
        [Required, MaxLength(300)]
        public string EDIUserID { get; set; }
        [MaxLength(100)]
        public string LastEvent { get; set; }
        public DateTime? LastEventDate { get; set; }
        [MaxLength(50)]
        public string OrganizationKPP { get; set; }
        [MaxLength(50)]
        public string OrganizationINN { get; set; }
        public bool LoadContracts { get; set; }
        public bool LoadBuhDocs { get; set; }
        [MaxLength(200)]
        public string OrganizationName { get; set; }
        public Client Client { get; set; }
    }
}
