using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QUANLYDIEMRENLUYEN.Models;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Models
{
    [Table("AccountProfile")]
    public class AccountProfile
    {
        [Key]
        public int ProfileId { get; set; }

        [Required]
        public int AccountId { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? NationalID { get; set; }
        public string? Nationality { get; set; }
        public string? Ethnicity { get; set; }
        public string? Religion { get; set; }

        [ForeignKey("AccountId")]
        public virtual Account? Account { get; set; }
    }
}