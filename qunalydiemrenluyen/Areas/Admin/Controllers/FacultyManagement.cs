using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYDIEMRENLUYEN.Models;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class FacultyManagementController : Controller
    {
        private readonly DataContext _context;

        public FacultyManagementController(DataContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách sinh viên, khoa, lớp
        public IActionResult Index()
        {
            // Lấy danh sách sinh viên bao gồm thông tin khoa và lớp
            var students = _context.Accounts
                .Include(a => a.Faculty)
                .Include(a => a.Class)
                .Where(a => a.Role == "Student" || a.Role == "Classmonitor")
                .ToList();

            ViewBag.Faculties = _context.Faculties.ToList();
            ViewBag.Classes = _context.Classes.Include(c => c.Faculty).ToList();
            return View(students);
        }

        // Gán khoa cho sinh viên
        [HttpPost]
        public IActionResult AssignFaculty(int accountId, int facultyId)
        {
            // Kiểm tra xem tài khoản có tồn tại không
            var acc = _context.Accounts.Find(accountId);
            if (acc != null)
            {
                acc.FacultyId = facultyId;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // Gán lớp cho sinh viên
        [HttpPost]
        public IActionResult AssignClass(int accountId, int classId)
        {
            // Kiểm tra xem tài khoản và lớp có tồn tại không
            var acc = _context.Accounts.Find(accountId);
            if (acc != null)
            {
                acc.ClassId = classId;
                // Nếu muốn tự động gán FacultyId theo lớp:
                var cls = _context.Classes.Find(classId);
                if (cls != null)
                {
                    if (cls.FacultyId.HasValue)
                    {
                        acc.FacultyId = cls.FacultyId;
                    }
                    if (acc.Role == "Classmonitor")
                    {
                        cls.ClassMonitorId = acc.AccountId;
                    }
                    else // Nếu là sinh viên mà đang là lớp trưởng thì xóa chức vụ lớp trưởng
                    {
                        if (cls.ClassMonitorId == acc.AccountId)
                        {
                            cls.ClassMonitorId = null;
                        }
                    }
                }
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}