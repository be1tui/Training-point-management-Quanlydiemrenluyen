using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYDIEMRENLUYEN.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QUANLYDIEMRENLUYEN.Controllers
{
    public class PercentageController : Controller
    {
        private readonly DataContext _context;
        public PercentageController(DataContext context)
        {
            _context = context;
        }

        // Xem tỉ lệ % phiếu sinh viên gửi cho lớp trưởng (đã nộp)
        public async Task<IActionResult> Index(int? facultyId, int? classId, int? academicYearId, int? semesterId)
        {
            // Lấy AccountId của lớp trưởng từ session
            int accountId = QUANLYDIEMRENLUYEN.Utilities.Functions.AccountId;
            var classMonitor = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == accountId && a.Role == "Classmonitor");
            if (classMonitor == null || classMonitor.FacultyId == null)
            {
                return Unauthorized();
            }
            facultyId = classMonitor.FacultyId; // Chỉ lấy theo khoa của lớp trưởng

            // Lấy danh sách filter (không cho chọn khoa khác)
            ViewBag.Faculties = await _context.Faculties.Where(f => f.FacultyId == facultyId).ToListAsync();
            ViewBag.Classes = await _context.Classes.Where(c => c.FacultyId == facultyId).ToListAsync();
            ViewBag.AcademicYears = await _context.AcademicYears.OrderByDescending(y => y.YearName).ToListAsync();
            ViewBag.Semesters = academicYearId.HasValue
                ? await _context.Semesters.Where(s => s.AcademicYearId == academicYearId).ToListAsync()
                : new List<QUANLYDIEMRENLUYEN.Areas.Admin.Models.Semester>();

            // Lấy danh sách phiếu đã nộp trong khoa của lớp trưởng
            var query = _context.StudentEvaluations
                .Include(e => e.Account)
                .Include(e => e.Semester)
                .Where(e => e.Account.FacultyId == facultyId && e.Account.ClassId == classMonitor.ClassId)
                .AsQueryable();
            // Lọc theo các điều kiện nếu có
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
            // Chỉ lấy những phiếu đang chờ lớp trưởng chấm hoặc sửa
            query = query.Where(e => e.Status == "Chờ lớp trưởng chấm" || e.Status == "Chờ lớp trưởng sửa");
            // Tính tổng số phiếu và phân loại theo Note
            var total = await query.CountAsync();
            var countXuatSac = await query.CountAsync(e => e.Note == "Xuất sắc");
            var countGioi = await query.CountAsync(e => e.Note == "Giỏi");
            var countKha = await query.CountAsync(e => e.Note == "Khá");
            var countTrungBinh = await query.CountAsync(e => e.Note == "Trung bình");
            var countYeu = await query.CountAsync(e => e.Note == "Yếu");
            // Truyền dữ liệu vào ViewBag
            ViewBag.Total = total;
            ViewBag.CountXuatSac = countXuatSac;
            ViewBag.CountGioi = countGioi;
            ViewBag.CountKha = countKha;
            ViewBag.CountTrungBinh = countTrungBinh;
            ViewBag.CountYeu = countYeu;

            return View();
        }
    }
}