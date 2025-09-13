using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYDIEMRENLUYEN.Areas.Admin.Models;
using QUANLYDIEMRENLUYEN.Models;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AcademicYearController : Controller
    {
        private readonly DataContext _context;

        public AcademicYearController(DataContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Lấy danh sách năm học và học kỳ từ cơ sở dữ liệu
                var academicYears = await _context.AcademicYears.OrderByDescending(y => y.YearName).ToListAsync();
                var semesters = await _context.Semesters
                    .Include(s => s.AcademicYear)
                    .OrderByDescending(s => s.SemesterName)
                    .ToListAsync();
                // Tạo mô hình Tuple để truyền dữ liệu đến View
                var model = Tuple.Create<IEnumerable<AcademicYear>, IEnumerable<Semester>>(academicYears, semesters);
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                TempData["Error"] = "Không thể tải danh sách.";
                var emptyModel = Tuple.Create(Enumerable.Empty<AcademicYear>(), Enumerable.Empty<Semester>());
                return View(emptyModel);
            }
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            // Tìm kiếm năm học theo ID
            var year = await _context.AcademicYears.FindAsync(id);
            if (year == null) return NotFound();

            return View(year);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AcademicYear year)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem năm học đã tồn tại chưa
                try
                {
                    _context.AcademicYears.Add(year);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    ModelState.AddModelError("", "Không thể tạo năm học.");
                }
            }
            return View(year);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var year = await _context.AcademicYears.FindAsync(id);
            if (year == null) return NotFound();

            return View(year);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AcademicYear year)
        {
            if (id != year.AcademicYearId) return NotFound();

            if (ModelState.IsValid)
            {
                // Kiểm tra xem năm học có hợp lệ không
                try
                {
                    _context.Update(year);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AcademicYearExists(year.AcademicYearId)) return NotFound();
                    throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    ModelState.AddModelError("", "Không thể cập nhật.");
                }
            }
            return View(year);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var year = await _context.AcademicYears.FindAsync(id);
            if (year == null) return NotFound();
            // Lấy danh sách các học kỳ liên quan đến năm học này
            var relatedSemesters = await _context.Semesters
                .Where(s => s.AcademicYearId == id)
                .Select(s => s.SemesterName).ToListAsync();

            ViewBag.RelatedSemesters = relatedSemesters;
            return View(year);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var year = await _context.AcademicYears.FindAsync(id);
            if (year == null)
            {
                TempData["Error"] = "Không tìm thấy năm học.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.AcademicYears.Remove(year);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa năm học và các học kỳ liên quan.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi xóa năm học: {ex.Message}");
                TempData["Error"] = "Đã xảy ra lỗi khi xóa.";
                return View(year);
            }
        }
        private bool AcademicYearExists(int id) => _context.AcademicYears.Any(e => e.AcademicYearId == id);
    }
}