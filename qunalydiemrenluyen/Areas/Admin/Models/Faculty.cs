using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QUANLYDIEMRENLUYEN.Models
{
    [Table("Faculty")]
    public class Faculty
    {
        [Key]
        public int FacultyId { get; set; }

        [Required]
        [StringLength(255)]
        public string? FacultyName { get; set; }

        public string? Description { get; set; }

        // Navigation properties
        public virtual ICollection<Class>? Classes { get; set; }
        public virtual ICollection<Account>? Accounts { get; set; }
    }
}