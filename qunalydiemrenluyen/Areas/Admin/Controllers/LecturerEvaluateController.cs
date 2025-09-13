using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYDIEMRENLUYEN.Models;
using QUANLYDIEMRENLUYEN.Areas.Admin.Models;
using QUANLYDIEMRENLUYEN.Utilities;
using System.Linq;
using System.Threading.Tasks;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LecturerEvaluateController : Controller
    {
        private readonly DataContext _context;
        public LecturerEvaluateController(DataContext context)
        {
            _context = context;
        }

        // Danh sách sinh viên chờ duyệt điểm rèn luyện của khoa
        public async Task<IActionResult>Index(int? academicYearId, int? semesterId, string? studentCode)
        {
            // Lấy thông tin giảng viên đăng nhập
            int accountId = Functions.AccountId;
            var lecturer = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == accountId);
            if (lecturer == null || lecturer.FacultyId == null) return Unauthorized();
            // Lấy danh sách năm học và học kỳ
            var academicYears = await _context.AcademicYears.OrderByDescending(y => y.YearName).ToListAsync();
            ViewBag.AcademicYears = academicYears;
            // Lấy danh sách học kỳ theo năm học đã chọn
            List<Semester> semesters = new List<Semester>();
            if (academicYearId.HasValue)
            {
                semesters = await _context.Semesters
                    .Where(s => s.AcademicYearId == academicYearId.Value)
                    .OrderByDescending(s => s.SemesterName)
                    .ToListAsync();
            }
            ViewBag.Semesters = semesters;
            ViewBag.SelectedAcademicYearId = academicYearId;
            ViewBag.SelectedSemesterId = semesterId;
            ViewBag.StudentCode = studentCode;
            // Lấy danh sách đánh giá sinh viên chờ duyệt của khoa
            var query = _context.StudentEvaluations
                .Include(e => e.Account)
                .Include(e => e.Semester)
                .Where(e => e.Account.FacultyId == lecturer.FacultyId && e.Status == "Chờ CNK duyệt") // Lấy các bản đánh giá của sinh viên thuộc khoa của giảng viên và đang chờ CNK duyệt
                .Join(_context.EvaluationConfigs,
                    e => e.SemesterId,
                    c => c.SemesterId,
                    (e, c) => new { Evaluation = e, Config = c })
                .Where(ec => ec.Config.LecturerEvalStart <= DateTime.Now && ec.Config.LecturerEvalEnd >= DateTime.Now) // Chỉ lấy các bản đánh giá trong thời gian cho phép giảng viên duyệt
                .Select(ec => ec.Evaluation);
            // Lọc theo mã sinh viên, học kỳ hoặc năm học nếu có
            if (!string.IsNullOrEmpty(studentCode))
            {
                query = query.Where(e => e.Account.Username.Contains(studentCode));
            }
            if (semesterId.HasValue)
            {
                query = query.Where(e => e.SemesterId == semesterId.Value);
            }
            else if (academicYearId.HasValue)
            {
                var semesterIds = semesters.Select(s => s.SemesterId).ToList();
                query = query.Where(e => semesterIds.Contains(e.SemesterId));
            }
            // Sắp xếp theo ngày nộp mới nhất
            var evaluations = await query.OrderByDescending(e => e.SubmitDate).ToListAsync();

            // Thống kê tỉ lệ xếp loại sinh viên tự chọn
            var total = evaluations.Count();
            var countXuatSac = evaluations.Count(e => e.Note == "Xuất sắc");
            var countGioi = evaluations.Count(e => e.Note == "Giỏi");
            var countKha = evaluations.Count(e => e.Note == "Khá");
            var countTrungBinh = evaluations.Count(e => e.Note == "Trung bình");
            var countYeu = evaluations.Count(e => e.Note == "Yếu");

            ViewBag.Total = total;
            ViewBag.CountXuatSac = countXuatSac;
            ViewBag.CountGioi = countGioi;
            ViewBag.CountKha = countKha;
            ViewBag.CountTrungBinh = countTrungBinh;
            ViewBag.CountYeu = countYeu;

            return View(evaluations);
        }

        // Xem chi tiết và nhập điểm duyệt
        public async Task<IActionResult>DetailEvaluate(int id)
        {
            // Kiểm tra giảng viên có quyền xem đánh giá này không
            var evaluation = await _context.StudentEvaluations
                .Include(e => e.Account)
                .Include(e => e.Semester)
                .FirstOrDefaultAsync(e => e.EvaluationId == id);
            if (evaluation == null) return NotFound();
            // Kiểm tra xem giảng viên có phải là giảng viên của khoa sinh viên này không
            var details = await _context.EvaluationDetails
                .Include(d => d.Criteria)
                .Where(d => d.EvaluationId == id)
                .ToListAsync();
            // Nếu không phải giảng viên của khoa sinh viên này thì trả về Không được phép
            var evidences = await _context.Evidences
                .Where(ev => ev.EvaluationId == id)
                .ToListAsync();

            ViewBag.Evaluation = evaluation;
            ViewBag.Evidences = evidences;
            return View(details);
        }

        // Lưu điểm duyệt của giảng viên
        [HttpPost]
        public async Task<IActionResult>Approve(int evaluationId, string? lecturerNote)
        {
            // Kiểm tra giảng viên có quyền duyệt đánh giá này không
            var details = await _context.EvaluationDetails
                .Where(d => d.EvaluationId == evaluationId)
                .ToListAsync();

            // Không cập nhật điểm từng mục, chỉ lưu ghi chú nếu có
            foreach (var detail in details)
            {
                detail.LecturerNote = lecturerNote;
                // Nếu muốn lưu điểm duyệt = điểm lớp trưởng:
                detail.LecturerScore = detail.ClassMonitorScore;
                detail.FinalScore = detail.ClassMonitorScore;
            }
            await _context.SaveChangesAsync();

            // Cập nhật trạng thái và tổng điểm
            var evaluation = await _context.StudentEvaluations.FirstOrDefaultAsync(e => e.EvaluationId == evaluationId);
            if (evaluation != null)
            {
                evaluation.Status = "Đã duyệt";
                // Tổng điểm = tổng điểm lớp trưởng
                int total = details.Sum(d => d.ClassMonitorScore ?? 0);

                // Xếp loại theo tổng điểm
                string rank;
                if (total >= 90)
                    rank = "Xuất sắc";
                else if (total >= 80)
                    rank = "Giỏi";
                else if (total >= 65)
                    rank = "Khá";
                else if (total >= 50)
                    rank = "Trung bình";
                else
                    rank = "Yếu";

                // Lưu tổng điểm vào EvaluationSummary
                var summary = await _context.EvaluationSummaries
                    .FirstOrDefaultAsync(s => s.AccountId == evaluation.AccountId && s.SemesterId == evaluation.SemesterId);
                if (summary == null)
                {
                    summary = new EvaluationSummary
                    {
                        AccountId = evaluation.AccountId,
                        SemesterId = evaluation.SemesterId,
                        TotalScore = total,
                        Rank = rank
                    };
                    _context.EvaluationSummaries.Add(summary);
                }
                else
                {
                    summary.TotalScore = total;
                    summary.Rank = rank;
                }
                await _context.SaveChangesAsync();
            }

            TempData["Message"] = "Đã duyệt điểm rèn luyện!";
            return RedirectToAction("Index");
        }

        // Từ chối duyệt
        [HttpPost]
        public async Task<IActionResult>Reject(int evaluationId, string note)
        {
            // Kiểm tra giảng viên có quyền từ chối đánh giá này không
            var evaluation = await _context.StudentEvaluations
                .Include(e => e.Account)
                .FirstOrDefaultAsync(e => e.EvaluationId == evaluationId);
            if (evaluation != null)
            {
                evaluation.Status = "Chờ lớp trưởng sửa"; // cập nhật trạng thái để lớp trưởng sửa
                evaluation.Note = note;
                await _context.SaveChangesAsync();

                // Gửi thông báo cho lớp trưởng
                var classMonitor = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.ClassId == evaluation.Account.ClassId && a.Role == "Classmonitor");
                if (classMonitor != null)
                {
                    var notify = new Notification
                    {
                        AccountId = classMonitor.AccountId,
                        Title = "Bảng đánh giá bị từ chối",
                        Message = $"Bảng đánh giá của sinh viên {evaluation.Account.FullName} bị từ chối: {note}",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    };
                    _context.Notifications.Add(notify);
                    await _context.SaveChangesAsync();
                }
            }
            TempData["Message"] = "Đã từ chối bản đánh giá và gửi lại cho lớp trưởng sửa!";
            return RedirectToAction("Index");
        }
    }
}