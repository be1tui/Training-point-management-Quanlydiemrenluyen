using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYDIEMRENLUYEN.Models;
using System.Linq;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ListStudentController : Controller
    {
        private readonly DataContext _context;
        public ListStudentController(DataContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? facultyId, int? classId, string? search)
        {
            // Lấy danh sách khoa
            var faculties = _context.Faculties.ToList();

            // Lấy danh sách lớp theo khoa (nếu có chọn khoa)
            var classes = _context.Classes
                .Where(c => !facultyId.HasValue || c.FacultyId == facultyId)
                .ToList();

            // Lấy danh sách sinh viên theo filter
            var students = _context.Accounts
                .Include(a => a.Class)
                .Include(a => a.Faculty)
                .Where(a => a.Role == "Student" || a.Role == "ClassMonitor")
                .Where(a => !facultyId.HasValue || a.FacultyId == facultyId)
                .Where(a => !classId.HasValue || a.ClassId == classId)
                .Where(a => string.IsNullOrEmpty(search) || 
                            a.FullName.Contains(search) || 
                            a.Username.Contains(search))
                .OrderBy(a => a.FullName)
                .ToList();

            ViewBag.Faculties = faculties;
            ViewBag.Classes = classes;
            ViewBag.SelectedFaculty = facultyId;
            ViewBag.SelectedClass = classId;
            ViewBag.Search = search;

            return View(students);
        }
    }
}