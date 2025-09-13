using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QUANLYDIEMRENLUYEN.Models;


namespace QUANLYDIEMRENLUYEN.Areas.Admin.Models
{
    [Table("SystemLog")]
    public class SystemLog
    {
        [Key]
        public int LogId { get; set; }
        public int? AccountId { get; set; }
        public string? Action { get; set; }
        public DateTime LogTime { get; set; } = DateTime.Now;

        [ForeignKey("AccountId")]
        public virtual Account? Account { get; set; }
    }
}