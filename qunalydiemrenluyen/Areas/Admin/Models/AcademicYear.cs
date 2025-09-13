using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Models
{
    [Table("AcademicYear")]
    public class AcademicYear
    {
        [Key]
        public int AcademicYearId { get; set; }

        [Required]
        [StringLength(20)]
        public string? YearName { get; set; }

        public virtual ICollection<Semester>? Semesters { get; set; }
    }
}
