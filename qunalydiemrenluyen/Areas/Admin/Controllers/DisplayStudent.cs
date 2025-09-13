using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYDIEMRENLUYEN.Models;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DisplayStudentController : Controller
    {
        private readonly DataContext _context;
        public DisplayStudentController(DataContext context)
        {
            _context = context;
        }

        // Danh sách sinh viên theo lớp/khoa
        public async Task<IActionResult> Index(int? classId, int? facultyId)
        {
            // Lấy danh sách sinh viên bao gồm thông tin lớp và khoa
            var students = _context.Accounts
                .Include(a => a.Class)
                .Include(a => a.Faculty)
                .Where(a => a.Role == "Student" || a.Role == "Classmonitor")
                .AsQueryable();
            // Lọc theo lớp nếu có
            if (classId.HasValue)
                students = students.Where(a => a.ClassId == classId);
            // Lọc theo khoa nếu có
            if (facultyId.HasValue)
                students = students.Where(a => a.FacultyId == facultyId);

            ViewBag.Classes = await _context.Classes.ToListAsync();
            ViewBag.Faculties = await _context.Faculties.ToListAsync();
            return View(await students.ToListAsync());
        }

        // GET: Thêm sinh viên
        public IActionResult AddStudent()
        {
            // Trả về danh sách lớp và khoa để hiển thị trong form thêm sinh viên
            ViewBag.Classes = _context.Classes.ToList();
            ViewBag.Faculties = _context.Faculties.ToList();
            return View();
        }

        // POST: Thêm sinh viên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(Account account)
        {
            // Kiểm tra xem tài khoản đã tồn tại chưa
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(account.Password))
                {
                    account.Password = QUANLYDIEMRENLUYEN.Utilities.Functions.MD5Password(account.Password);
                }
                // Role lấy từ form, không cần gán cứng
                _context.Accounts.Add(account);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Classes = _context.Classes.ToList();
            ViewBag.Faculties = _context.Faculties.ToList();
            return View(account);
        }


        // GET: Sửa sinh viên
        public async Task<IActionResult> EditStudent(int id)
        {
            // Tìm kiếm tài khoản sinh viên theo id
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            ViewBag.Classes = _context.Classes.ToList();
            ViewBag.Faculties = _context.Faculties.ToList();
            return View(account);
        }

        // POST: Sửa sinh viên
        [HttpPost]
        public async Task<IActionResult> EditStudent(int id, Account account)
        {
            if (id != account.AccountId)
            {
                return BadRequest();
            }
            // Kiểm tra xem tài khoản có hợp lệ không
            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Accounts.FindAsync(id);
                    if (existing == null)
                        return NotFound();

                    // Cập nhật các trường
                    existing.Username = account.Username;
                    existing.FullName = account.FullName;
                    existing.Email = account.Email;
                    existing.ClassId = account.ClassId;
                    existing.FacultyId = account.FacultyId;
                    existing.Role = account.Role;
                    existing.IsActive = account.IsActive;

                    // Nếu nhập mật khẩu mới thì mã hóa lại
                    if (!string.IsNullOrEmpty(account.Password))
                    {
                        existing.Password = QUANLYDIEMRENLUYEN.Utilities.Functions.MD5Password(account.Password);
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                // Xử lý ngoại lệ nếu có lỗi cập nhật
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Accounts.Any(e => e.AccountId == id))
                        return NotFound();
                    throw;
                }
            }
            ViewBag.Classes = _context.Classes.ToList();
            ViewBag.Faculties = _context.Faculties.ToList();
            return View(account);
        }

        // Xóa sinh viên
        [HttpPost]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account != null)
            {
            
                _context.Accounts.Remove(account);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}