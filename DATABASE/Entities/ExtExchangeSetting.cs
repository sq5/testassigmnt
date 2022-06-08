using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARCHIVE.COMMON.Entities
{
    public class ExtExchangeSetting
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public bool SyncContracts { get; set; }
        [Required]
        public bool SyncOnlyApprovedContracts { get; set; }
        [Required]
        public bool SyncInvoices { get; set; }
        [Required]
        public bool SyncOnlyApprovedInvoices { get; set; }
        public bool? NotifyEmailSender { get; set; }
        [Required]
        public int ClientId { get; set; }
        public Client Client { get; set; }
        public bool? SyncInputs { get; set; }
        public bool? SyncOnlyApprovedInputs { get; set; }

    }
}
