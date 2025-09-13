using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYDIEMRENLUYEN.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminPercentageController : Controller
    {
        private readonly DataContext _context;
        public AdminPercentageController(DataContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? facultyId, int? classId, int? academicYearId, int? semesterId)
        {
            // Lấy danh sách filter
            ViewBag.Faculties = await _context.Faculties.ToListAsync();
            ViewBag.Classes = facultyId.HasValue
                ? await _context.Classes.Where(c => c.FacultyId == facultyId).ToListAsync()
                : new List<Class>();
            ViewBag.AcademicYears = await _context.AcademicYears.OrderByDescending(y => y.YearName).ToListAsync();
            ViewBag.Semesters = academicYearId.HasValue
                ? await _context.Semesters.Where(s => s.AcademicYearId == academicYearId).ToListAsync()
                : new List<QUANLYDIEMRENLUYEN.Areas.Admin.Models.Semester>();

            // Lấy danh sách phiếu đã duyệt theo filter
            var query = _context.EvaluationSummaries
                .Include(e => e.Account)
                .Include(e => e.Semester)
                .AsQueryable();
            // Lọc theo khoa, lớp, học kỳ hoặc năm học nếu có
            if (facultyId.HasValue)
                query = query.Where(e => e.Account.FacultyId == facultyId);
            if (classId.HasValue)
                query = query.Where(e => e.Account.ClassId == classId);
            if (semesterId.HasValue)
                query = query.Where(e => e.SemesterId == semesterId);
            else if (academicYearId.HasValue)
            {
                var semesterIds = await _context.Semesters
                    .Where(s => s.AcademicYearId == academicYearId)
                    .Select(s => s.SemesterId)
                    .ToListAsync();
                query = query.Where(e => semesterIds.Contains(e.SemesterId));
            }
            // Chỉ lấy những phiếu đã duyệt
            var total = await query.CountAsync();
            var countXuatSac = await query.CountAsync(e => e.Rank == "Xuất sắc");
            var countGioi = await query.CountAsync(e => e.Rank == "Giỏi");
            var countKha = await query.CountAsync(e => e.Rank == "Khá");
            var countTrungBinh = await query.CountAsync(e => e.Rank == "Trung bình");
            var countYeu = await query.CountAsync(e => e.Rank == "Yếu");

            // Trả về dữ liệu cho view
            ViewBag.Total = total;
            ViewBag.CountXuatSac = countXuatSac;
            ViewBag.CountGioi = countGioi;
            ViewBag.CountKha = countKha;
            ViewBag.CountTrungBinh = countTrungBinh;
            ViewBag.CountYeu = countYeu;

            return View();
        }

        // Gửi thông báo cho lớp trưởng
        [HttpPost]
        public async Task<IActionResult> SendNotify(int classId, string message)
        {
            // Kiểm tra xem lớp trưởng có tồn tại không
            var classMonitor = await _context.Accounts
                .FirstOrDefaultAsync(a => a.ClassId == classId && a.Role == "Classmonitor");
            if (classMonitor != null)
            {
                // Gửi thông báo cho lớp trưởng
                _context.Notifications.Add(new Areas.Admin.Models.Notification
                {
                    AccountId = classMonitor.AccountId,
                    Title = "Yêu cầu đánh giá lại bảng điểm rèn luyện",
                    Message = message,
                    CreatedAt = DateTime.Now,
                    IsRead = false
                });
                await _context.SaveChangesAsync();
                TempData["Message"] = "Đã gửi thông báo cho lớp trưởng!";
            }
            return RedirectToAction("Index");
        }
    }
}