using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QUANLYDIEMRENLUYEN.Areas.Admin.Models;
using QUANLYDIEMRENLUYEN.Models;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SemesterController : Controller
    {
        private readonly DataContext _context;

        public SemesterController(DataContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy danh sách học kỳ và bao gồm thông tin năm học
            var semesters = await _context.Semesters.Include(s => s.AcademicYear).ToListAsync();
            return View(semesters);
        }

        public async Task<IActionResult> Details(int? id)
        {
            // Kiểm tra xem id có null không
            if (id == null) return NotFound();
            // Tìm kiếm học kỳ theo id và bao gồm thông tin năm học
            var semester = await _context.Semesters.Include(s => s.AcademicYear)
                .FirstOrDefaultAsync(m => m.SemesterId == id);
            if (semester == null) return NotFound();

            return View(semester);
        }

        public IActionResult Create()
        {
            // Tạo ViewBag cho danh sách năm học để hiển thị trong dropdown
            ViewBag.AcademicYears = new SelectList(_context.AcademicYears, "AcademicYearId", "YearName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Semester semester)
        {
            // Kiểm tra xem học kỳ đã tồn tại trong năm học chưa
            if (ModelState.IsValid)
            {
                _context.Semesters.Add(semester);
                _context.SaveChanges();
                return RedirectToAction("Index", "AcademicYear", new { area = "Admin" });
            }
            // Nếu có lỗi, tạo lại ViewBag cho danh sách năm học
            ViewBag.AcademicYears = new SelectList(_context.AcademicYears, "AcademicYearId", "YearName", semester.AcademicYearId);
            return View(semester);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var semester = await _context.Semesters.FindAsync(id);
            if (semester == null) return NotFound();

            ViewBag.AcademicYears = new SelectList(_context.AcademicYears, "AcademicYearId", "YearName", semester.AcademicYearId);
            return View(semester);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Semester semester)
        {
            // Kiểm tra xem id có khớp với học kỳ đang chỉnh sửa không
            if (id != semester.SemesterId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(semester);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "AcademicYear", new { area = "Admin" });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    ModelState.AddModelError("", "Không thể cập nhật học kỳ.");
                }
            }

            ViewBag.AcademicYears = new SelectList(_context.AcademicYears, "AcademicYearId", "YearName", semester.AcademicYearId);
            return View(semester);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var semester = await _context.Semesters.Include(s => s.AcademicYear)
                .FirstOrDefaultAsync(m => m.SemesterId == id);
            if (semester == null) return NotFound();

            return View(semester);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var semester = await _context.Semesters.FindAsync(id);
            if (semester == null) return NotFound();

            try
            {
                _context.Semesters.Remove(semester);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa học kỳ.";
                return RedirectToAction("Index", "AcademicYear", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                TempData["Error"] = "Không thể xóa học kỳ.";
                return RedirectToAction("Index", "AcademicYear", new { area = "Admin" });
            }
        }
    }
}
