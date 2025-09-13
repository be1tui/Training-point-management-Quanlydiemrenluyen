using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QUANLYDIEMRENLUYEN.Models;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Models
{
    [Table("Attendance")]
    public class Attendance
    {
        [Key]
        public int AttendanceId { get; set; }

        public int ClassId { get; set; }
        public DateTime SessionDate { get; set; }
        public string? QRCode { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("ClassId")]
        public virtual Class? Class { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual Account? Creator { get; set; }
    }
}