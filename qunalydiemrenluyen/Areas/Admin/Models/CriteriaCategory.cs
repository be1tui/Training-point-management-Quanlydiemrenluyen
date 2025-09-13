using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Models
{
    [Table("CriteriaCategory")]
    public class CriteriaCategory
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        public string? CategoryName { get; set; }
        public int MaxScore { get; set; }
        public virtual ICollection<Criteria>? Criterias { get; set; }
    }
}