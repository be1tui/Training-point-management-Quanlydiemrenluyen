using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Models
{
    [Table("EvaluationDetail")]
    public class EvaluationDetail
    {
        [Key]
        public int DetailId { get; set; }

        public int EvaluationId { get; set; }
        public int CriteriaId { get; set; }
        public int? StudentScore { get; set; }
        public int? ClassMonitorScore { get; set; }
        public int? LecturerScore { get; set; }
        public int? FinalScore { get; set; }
        public string? LecturerNote { get; set; }

        [ForeignKey("EvaluationId")]
        public virtual StudentEvaluation? StudentEvaluation { get; set; }

        [ForeignKey("CriteriaId")]
        public virtual Criteria? Criteria { get; set; }
    }
}