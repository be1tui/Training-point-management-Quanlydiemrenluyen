using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Models
{
    public class ExportFileViewModel
    {
        public int? AcademicYearId { get; set; }
        public int? SemesterId { get; set; }
        public int? FacultyId { get; set; }
        public int? ClassId { get; set; }

        public List<SelectListItem> AcademicYears { get; set; } = new();
        public List<SelectListItem> Semesters { get; set; } = new();
        public List<SelectListItem> Faculties { get; set; } = new();
        public List<SelectListItem> Classes { get; set; } = new();
        public List<EvaluationSummary> Students { get; set; } = new();
    }
}