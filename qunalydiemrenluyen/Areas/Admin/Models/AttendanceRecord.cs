using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QUANLYDIEMRENLUYEN.Models;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Models
{
    [Table("AttendanceRecord")]
    public class AttendanceRecord
    {
        [Key]
        public int RecordId { get; set; }

        public int AttendanceId { get; set; }
        public int StudentId { get; set; }
        public DateTime CheckInTime { get; set; } = DateTime.Now;

        [ForeignKey("AttendanceId")]
        public virtual Attendance? Attendance { get; set; }

        [ForeignKey("StudentId")]
        public virtual Account? Student { get; set; }
    }
}