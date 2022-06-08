using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARCHIVE.COMMON.Entities
{
    public class OrgPerms
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        [Required, MaxLength(100)]
        public string User { get; set; }
    }
}
