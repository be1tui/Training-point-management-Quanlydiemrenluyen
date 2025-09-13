using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QUANLYDIEMRENLUYEN.Models
{
    [Table("Class")]
    public class Class
    {
        [Key]
        public int ClassId { get; set; }

        [Required]
        [StringLength(50)]
        public string? ClassName { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Thêm thuộc tính ClassMonitorId
        public int? ClassMonitorId { get; set; }

        // Thêm khoá ngoại FacultyId
        public int? FacultyId { get; set; }

        // Navigation property cho lớp trưởng
        [ForeignKey("ClassMonitorId")]
        public virtual Account? ClassMonitor { get; set; }

        [ForeignKey("FacultyId")]
        public virtual Faculty? Faculty { get; set; }
        
        public virtual ICollection<Account>? Accounts { get; set; }
    }
}