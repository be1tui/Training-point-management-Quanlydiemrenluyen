using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Models
{
    [Table("Evidence")]
    public class Evidence
    {
        [Key]
        public int EvidenceId { get; set; }

        public int EvaluationId { get; set; }
        public int CriteriaId { get; set; }
        public string? FilePath { get; set; }
        public string? Description { get; set; }
        public DateTime UploadDate { get; set; } = DateTime.Now;

        [ForeignKey("EvaluationId")]
        public virtual StudentEvaluation? StudentEvaluation { get; set; }

        [ForeignKey("CriteriaId")]
        public virtual Criteria? Criteria { get; set; }
    }
}