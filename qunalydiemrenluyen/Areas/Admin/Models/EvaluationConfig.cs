using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Models
{
    [Table("EvaluationConfig")]
    public class EvaluationConfig
    {
        [Key]
        public int ConfigId { get; set; }

        public int SemesterId { get; set; }
        public DateTime? SelfEvalStart { get; set; }
        public DateTime? SelfEvalEnd { get; set; }
        public DateTime? LecturerEvalStart { get; set; }
        public DateTime? LecturerEvalEnd { get; set; }

        [ForeignKey("SemesterId")]
        public virtual Semester? Semester { get; set; }
    }
}