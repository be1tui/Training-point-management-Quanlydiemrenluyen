using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYDIEMRENLUYEN.Models;
using QUANLYDIEMRENLUYEN.Areas.Admin.Models;
using QUANLYDIEMRENLUYEN.Utilities;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace QUANLYDIEMRENLUYEN.Controllers
{
    public class EvaluateController : Controller
    {
        private readonly DataContext _context;

        public EvaluateController(DataContext context)
        {
            _context = context;
        }

        // Hiển thị form tự đánh giá cho sinh viên
        public async Task<IActionResult> Index()
        {
            int accountId = Functions.AccountId;
            if (accountId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy học kỳ đang mở đánh giá
            var config = await _context.EvaluationConfigs
                .Include(c => c.Semester)
                .OrderByDescending(c => c.ConfigId)
                .FirstOrDefaultAsync(c => c.SelfEvalStart <= DateTime.Now && c.SelfEvalEnd >= DateTime.Now);

            if (config == null)
            {
                ViewBag.Message = "Hiện tại chưa đến thời gian tự đánh giá.";
                return View(new List<CriteriaCategory>());
            }

            // Lấy các tiêu chí theo danh mục
            var categories = await _context.CriteriaCategories
                .Include(c => c.Criterias)
                .ToListAsync();

            // Kiểm tra sinh viên đã nộp chưa
            var submitted = await _context.StudentEvaluations
                .AnyAsync(e => e.AccountId == accountId && e.SemesterId == config.SemesterId);

            ViewBag.Submitted = submitted;
            ViewBag.SemesterId = config.SemesterId;
            ViewBag.Message = TempData["Message"];
            return View(categories);
        }

        // Xử lý lưu tự đánh giá
        [HttpPost]
        public async Task<IActionResult> Submit(int semesterId, Dictionary<int, int> scores, string? note)
        {
            // Kiểm tra tài khoản đã đăng nhập
            int accountId = Functions.AccountId;
            if (accountId == 0)
                return RedirectToAction("Login", "Account");

            // Kiểm tra đã nộp chưa
            var existed = await _context.StudentEvaluations
                .FirstOrDefaultAsync(e => e.AccountId == accountId && e.SemesterId == semesterId);
            if (existed != null)
            {
                TempData["Message"] = "Bạn đã nộp bản tự đánh giá trước đó.";
                return RedirectToAction("Index");
            }

            // Tính tổng điểm tự đánh giá và xếp loại
            int totalScore = scores.Values.Sum();
            string rank = GetRank(totalScore);

            // Lưu bản tự đánh giá
            var evaluation = new StudentEvaluation
            {
                AccountId = accountId,
                SemesterId = semesterId,
                Status = "Chờ lớp trưởng chấm",
                Note = rank // Lưu xếp loại vào Note để lớp trưởng xem
            };
            _context.StudentEvaluations.Add(evaluation);
            await _context.SaveChangesAsync();

            // Lưu chi tiết điểm từng tiêu chí và minh chứng nếu có
            foreach (var item in scores)
            {
                var detail = new EvaluationDetail
                {
                    EvaluationId = evaluation.EvaluationId,
                    CriteriaId = item.Key,
                    StudentScore = item.Value
                };
                _context.EvaluationDetails.Add(detail);

                // Xử lý file minh chứng nếu có
                var file = Request.Form.Files.FirstOrDefault(f => f.Name == $"EvidenceFiles[{item.Key}]");
                if (file != null && file.Length > 0)
                {
                    // Tạo thư mục nếu chưa có
                    var evidenceDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "Evidence");
                    if (!Directory.Exists(evidenceDir))
                        Directory.CreateDirectory(evidenceDir);

                    // Tạo tên file duy nhất
                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                    var filePath = Path.Combine(evidenceDir, fileName);

                    // Lưu file lên server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Lưu thông tin vào bảng Evidence
                    var evidence = new Evidence
                    {
                        EvaluationId = evaluation.EvaluationId,
                        CriteriaId = item.Key,
                        FilePath = $"/uploads/Evidence/{fileName}",
                        UploadDate = DateTime.Now
                    };
                    _context.Evidences.Add(evidence);
                }
            }
            await _context.SaveChangesAsync();

            // Gửi thông báo cho lớp trưởng
            var student = await _context.Accounts.Include(a => a.Class).FirstOrDefaultAsync(a => a.AccountId == accountId);
            var classMonitorId = student?.Class?.ClassMonitorId;
            if (classMonitorId.HasValue)
            {
                var notify = new Notification
                {
                    AccountId = classMonitorId.Value,
                    Title = "Có bản tự đánh giá mới cần duyệt",
                    Message = $"Sinh viên {student.FullName} vừa nộp bản tự đánh giá.",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };
                _context.Notifications.Add(notify);
                await _context.SaveChangesAsync();
            }

            TempData["Message"] = "Nộp bản tự đánh giá thành công!";
            return RedirectToAction("Index");
        }
        private string GetRank(int totalScore)
        {
            if (totalScore >= 90) return "Xuất sắc";
            if (totalScore >= 80) return "Giỏi";
            if (totalScore >= 65) return "Khá";
            if (totalScore >= 50) return "Trung bình";
            return "Yếu";
        }
    }
}