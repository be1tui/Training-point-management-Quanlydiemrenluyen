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
    public class ClassController : Controller
    {
        private readonly DataContext _context;

        public ClassController(DataContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách lớp học từ cơ sở dữ liệu
            try
            {
                var classList = await _context.Classes.OrderBy(c => c.ClassId).ToListAsync();
                return View(classList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                TempData["Error"] = "Không thể tải danh sách lớp học. Vui lòng kiểm tra cơ sở dữ liệu.";
                return View(new List<Class>());
            }
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            // Tìm kiếm lớp học theo ID
            var classItem = await _context.Classes.FirstOrDefaultAsync(m => m.ClassId == id);
            if (classItem == null)
            {
                return NotFound();
            }

            return View(classItem);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Class classItem)
        {
            // Kiểm tra xem lớp học đã tồn tại chưa
            if (ModelState.IsValid)
            {
                try
                {
                    // Gán thời gian hiện tại cho CreatedAt
                    classItem.CreatedAt = DateTime.Now;
                    _context.Classes.Add(classItem);
                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    ModelState.AddModelError("", "Không thể tạo lớp học. Vui lòng thử lại.");
                }
            }
            return View(classItem);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            // Tìm kiếm lớp học theo ID
            var classItem = await _context.Classes.FindAsync(id);
            if (classItem == null)
            {
                return NotFound();
            }
            return View(classItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ClassId,ClassName,Description,CreatedAt")] Class classItem)
        {
            if (id != classItem.ClassId)
            {
                return NotFound();
            }
            // Kiểm tra xem lớp học có hợp lệ không
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(classItem);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClassExists(classItem.ClassId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    ModelState.AddModelError("", "Không thể cập nhật lớp học. Vui lòng thử lại.");
                }
            }
            return View(classItem);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            // Tìm kiếm lớp học theo ID và bao gồm danh sách sinh viên
            var classItem = await _context.Classes
                .Include(c => c.Accounts) // Include để load danh sách sinh viên
                .FirstOrDefaultAsync(c => c.ClassId == id);
            // Kiểm tra xem lớp học có tồn tại không
            if (classItem == null)
            {
                return NotFound();
            }

            return View(classItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            // Kiểm tra xem ID có hợp lệ không
            var classItem = _context.Classes.Find(id);
            if (classItem == null)
            {
                return NotFound();
            }
            // Kiểm tra xem lớp học có sinh viên không
            try
            {
                // Xóa tất cả account thuộc lớp này
                var accounts = _context.Accounts.Where(a => a.ClassId == id).ToList();
                _context.Accounts.RemoveRange(accounts);

                // Xóa lớp học
                _context.Classes.Remove(classItem);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                TempData["Error"] = "Không thể xóa lớp học. Vui lòng thử lại.";
                return RedirectToAction("Index");
            }
        }


        private bool ClassExists(int id)
        {
            return _context.Classes.Any(e => e.ClassId == id);
        }
    }
}