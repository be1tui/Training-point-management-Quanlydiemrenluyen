using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QUANLYDIEMRENLUYEN.Models;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Models
{
    [Table("EvaluationSummary")]
    public class EvaluationSummary
    {
        [Key]
        public int SummaryId { get; set; }

        public int AccountId { get; set; }
        public int SemesterId { get; set; }
        public int? TotalScore { get; set; }
        public string? Rank { get; set; }

        [ForeignKey("AccountId")]
        public virtual Account? Account { get; set; }

        [ForeignKey("SemesterId")]
        public virtual Semester? Semester { get; set; }
    }
}