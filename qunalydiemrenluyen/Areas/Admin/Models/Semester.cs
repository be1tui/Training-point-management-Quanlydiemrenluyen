using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Models
{
    [Table("Semester")]
    public class Semester
    {
        [Key]
        public int SemesterId { get; set; }

        [Required]
        [StringLength(20)]
        public string? SemesterName { get; set; }

        public int AcademicYearId { get; set; }

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear? AcademicYear { get; set; }
        public virtual ICollection<StudentEvaluation>? StudentEvaluations { get; set; }
        public virtual ICollection<EvaluationConfig>? EvaluationConfigs { get; set; }
    }
}
