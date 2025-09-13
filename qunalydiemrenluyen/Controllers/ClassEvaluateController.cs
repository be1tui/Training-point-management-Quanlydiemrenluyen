using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYDIEMRENLUYEN.Models;
using QUANLYDIEMRENLUYEN.Areas.Admin.Models;
using QUANLYDIEMRENLUYEN.Utilities;
using System.Linq;
using System.Threading.Tasks;

namespace QUANLYDIEMRENLUYEN.Controllers
{
    public class ClassEvaluateController : Controller
    {
        private readonly DataContext _context;
        public ClassEvaluateController(DataContext context)
        {
            _context = context;
        }

        // Danh sách sinh viên đã nộp tự đánh giá trong lớp của lớp trưởng
        public async Task<IActionResult> Index(int? academicYearId, int? semesterId)
        {
            // Kiểm tra lớp trưởng
            int accountId = Functions.AccountId;
            var classMonitor = await _context.Accounts.Include(a => a.Class)
                .FirstOrDefaultAsync(a => a.AccountId == accountId && a.Role == "Classmonitor");
            // Nếu không phải lớp trưởng hoặc không có lớp thì trả về Unauthorized
            if (classMonitor?.ClassId == null)
                return Unauthorized();

            // Lấy danh sách năm học và học kỳ
            var academicYears = await _context.AcademicYears.ToListAsync();
            ViewBag.AcademicYears = academicYears;
            // Lấy danh sách học kỳ theo năm học đã chọn
            var semesters = new List<Semester>();
            if (academicYearId.HasValue)
            {
                semesters = await _context.Semesters
                    .Where(s => s.AcademicYearId == academicYearId.Value)
                    .ToListAsync();
            }
            ViewBag.Semesters = semesters;
            ViewBag.SelectedAcademicYearId = academicYearId;
            ViewBag.SelectedSemesterId = semesterId;

            // Lọc theo năm học và học kỳ nếu có chọn
            var studentsQuery = _context.StudentEvaluations
                .Include(e => e.Account)
                .Where(e =>
                    e.Account.ClassId == classMonitor.ClassId &&
                    e.Account.FacultyId == classMonitor.FacultyId &&
                    (e.Status == "Chờ lớp trưởng chấm" || e.Status == "Chờ lớp trưởng sửa"));
                    
            // Lọc theo năm học
            if (semesterId.HasValue)
            {
                studentsQuery = studentsQuery.Where(e => e.SemesterId == semesterId.Value);
            }
            // Lọc theo học kỳ
            var students = await studentsQuery
                .OrderByDescending(e => e.SubmitDate)
                .ToListAsync();

            return View(students);
        }

        // Xem chi tiết và chấm điểm lớp cho từng sinh viên
        public async Task<IActionResult> Detail(int id)
        {
            // Kiểm tra lớp trưởng
            var evaluation = await _context.StudentEvaluations
                .Include(e => e.Account)
                .FirstOrDefaultAsync(e => e.EvaluationId == id);

            if (evaluation == null) return NotFound();
            // Kiểm tra xem lớp trưởng có quyền xem đánh giá này không
            var details = await _context.EvaluationDetails
                .Include(d => d.Criteria)
                .Where(d => d.EvaluationId == id)
                .ToListAsync();
            // Nếu không phải lớp trưởng của lớp sinh viên này thì trả về Unauthorized
            var evidences = await _context.Evidences
                .Where(ev => ev.EvaluationId == id)
                .ToListAsync();

            ViewBag.Evidences = evidences;
            ViewBag.Evaluation = evaluation;
            return View(details);
        }

        // Lưu điểm lớp đánh giá
        [HttpPost]
        public async Task<IActionResult> SaveClassScore(int evaluationId, Dictionary<int, int> classScores)
        {
            // Kiểm tra lớp trưởng
            var details = await _context.EvaluationDetails
                .Where(d => d.EvaluationId == evaluationId)
                .ToListAsync();
            // Nếu không có chi tiết đánh giá thì trả về NotFound
            foreach (var detail in details)
            {
                if (classScores.ContainsKey(detail.CriteriaId))
                {
                    detail.ClassMonitorScore = classScores[detail.CriteriaId];
                }
            }
            await _context.SaveChangesAsync();

            // Tính tổng điểm lớp đánh giá
            int total = details.Sum(d => d.ClassMonitorScore ?? 0);

            // Tính xếp loại tạm thời dựa trên tổng điểm
            string rank = "";
            if (total >= 90) rank = "Xuất sắc";
            else if (total >= 80) rank = "Giỏi";
            else if (total >= 65) rank = "Khá";
            else if (total >= 50) rank = "Trung bình";
            else rank = "Yếu";

            // Lưu tổng điểm vào EvaluationSummary (nếu có)
            var evaluation = await _context.StudentEvaluations.FirstOrDefaultAsync(e => e.EvaluationId == evaluationId);
            if (evaluation != null)
            {
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
                evaluation.Status = "Chờ CNK duyệt";
                await _context.SaveChangesAsync();

                // Gửi thông báo cho Chủ nhiệm khoa
                var student = await _context.Accounts.Include(a => a.Faculty).FirstOrDefaultAsync(a => a.AccountId == evaluation.AccountId);
                var facultyHead = await _context.Accounts.FirstOrDefaultAsync(a => a.FacultyId == student.FacultyId && a.Role == "FacultyHead");
                if (facultyHead != null)
                {
                    var notify = new Notification
                    {
                        AccountId = facultyHead.AccountId,
                        Title = "Có bản đánh giá cần duyệt",
                        Message = $"Sinh viên {student.FullName} đã được lớp trưởng chấm điểm. Tổng điểm: {total}",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    };
                    _context.Notifications.Add(notify);
                    await _context.SaveChangesAsync();
                }
            }

            TempData["Message"] = "Đã lưu điểm lớp đánh giá và gửi cho Chủ nhiệm khoa duyệt!";
            return RedirectToAction("Index");
        }
    }
}