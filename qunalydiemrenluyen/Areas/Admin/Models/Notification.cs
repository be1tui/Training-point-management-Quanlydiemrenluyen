using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QUANLYDIEMRENLUYEN.Models;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Models
{
    [Table("Notification")]
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        public int? AccountId { get; set; }
        [StringLength(255)]
        public string? Title { get; set; }
        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;

        [ForeignKey("AccountId")]
        public virtual Account? Account { get; set; }
    }
}