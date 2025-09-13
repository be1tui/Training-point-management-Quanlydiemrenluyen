using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QUANLYDIEMRENLUYEN.Models; 

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Models
{
    [Table("MeetingNotification")]
    public class MeetingNotification
    {
        [Key]
        public int MeetingId { get; set; }
        public int? AccountId { get; set; } // Thêm nếu chưa có

        [Required(ErrorMessage = "Vui lòng chọn Khoa/Viện.")]
        public int FacultyId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Lớp.")]
        public int ClassId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Năm học.")]
        public int AcademicYearId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Học kỳ.")]
        public int SemesterId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Ngày họp.")]
        [DataType(DataType.Date)]
        public DateTime MeetingDate { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Thời gian họp.")]
        [StringLength(20)]
        public string? MeetingTime { get; set; } 

        [StringLength(255, ErrorMessage = "Địa điểm không được vượt quá 255 ký tự.")]
        [Required(ErrorMessage = "Vui lòng nhập Địa điểm họp.")]
        public string? Location { get; set; }

        public string? Note { get; set; }

        public int? CreatedBy { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("FacultyId")]
        public virtual Faculty? Faculty { get; set; }

        [ForeignKey("ClassId")]
        public virtual Class? Class { get; set; }

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear? AcademicYear { get; set; }

        [ForeignKey("SemesterId")]
        public virtual Semester? Semester { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual Account? UserCreatedBy { get; set; }

        [ForeignKey("AccountId")]
        public virtual Account? Account { get; set; }
    }
}