using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Models
{
    [Table("Criteria")]
    public class Criteria
    {
        [Key]
        public int CriteriaId { get; set; }

        public int CategoryId { get; set; }
        public string? CriteriaName { get; set; }
        public int MaxScore { get; set; }
        public bool RequiresEvidence { get; set; }
        public string? EvaluationMethod { get; set; }

        [ForeignKey("CategoryId")]
        public virtual CriteriaCategory? Category { get; set; }
    }
}