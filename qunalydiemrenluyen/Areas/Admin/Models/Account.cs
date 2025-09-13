using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QUANLYDIEMRENLUYEN.Areas.Admin.Models;

namespace QUANLYDIEMRENLUYEN.Models
{
    [Table("Account")]
    public class Account
    {
        [Key]
        public int AccountId { get; set; }

        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Password { get; set; }
        public string? Avatar { get; set; }
        public string? Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }

        // Khoá ngoại có thể null
        public int? ClassId { get; set; }

        // Thêm khoá ngoại FacultyId có thể null
        public int? FacultyId { get; set; }

        // Thuộc tính điều hướng (navigation)
        [ForeignKey("ClassId")]
        public virtual Class? Class { get; set; }

        [ForeignKey("FacultyId")]
        public virtual Faculty? Faculty { get; set; }

        public virtual ICollection<StudentEvaluation>? StudentEvaluations { get; set; }
        public virtual ICollection<Notification>? Notifications { get; set; }
        public virtual ICollection<MeetingNotification>? MeetingNotifications { get; set; }
        public virtual ICollection<EvaluationSummary>? EvaluationSummaries { get; set; }
    }
}
