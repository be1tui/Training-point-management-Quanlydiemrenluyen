using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QUANLYDIEMRENLUYEN.Models;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Models
{
    [Table("StudentEvaluation")]
    public class StudentEvaluation
    {
        [Key]
        public int EvaluationId { get; set; }

        public int AccountId { get; set; }
        public int SemesterId { get; set; }
        public DateTime SubmitDate { get; set; } = DateTime.Now;
        public string? Status { get; set; }
        public string? Note { get; set; }

        [ForeignKey("AccountId")]
        public virtual Account? Account { get; set; }

        [ForeignKey("SemesterId")]
        public virtual Semester? Semester { get; set; }
    }
}