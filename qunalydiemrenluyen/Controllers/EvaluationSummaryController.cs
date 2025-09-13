using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYDIEMRENLUYEN.Models;
using QUANLYDIEMRENLUYEN.Areas.Admin.Models;
using QUANLYDIEMRENLUYEN.Utilities;
using System.Linq;
using System.Threading.Tasks;

namespace QUANLYDIEMRENLUYEN.Controllers
{
    public class EvaluationSummaryController : Controller
    {
        private readonly DataContext _context;
        public EvaluationSummaryController(DataContext context)
        {
            _context = context;
        }

        // Xem tổng điểm rèn luyện theo năm và học kỳ
        public async Task<IActionResult> Index()
        {
            int accountId = Functions.AccountId;
            // Lấy các EvaluationSummary chỉ khi StudentEvaluation đã được duyệt
            var summaries = await _context.EvaluationSummaries
                .Include(s => s.Semester)
                    .ThenInclude(se => se.AcademicYear)
                .Where(s => s.AccountId == accountId
                    && _context.StudentEvaluations.Any(e =>
                        e.AccountId == s.AccountId
                        && e.SemesterId == s.SemesterId
                        && e.Status == "Đã duyệt"))
                .OrderBy(s => s.Semester.AcademicYear.YearName)
                .ThenBy(s => s.Semester.SemesterName)
                .ToListAsync();

            // Tạo ViewModel đơn giản để tránh vòng lặp tham chiếu
            var grouped = summaries
                .GroupBy(s => s.Semester.AcademicYear.YearName)
                .Select(g =>
                {
                    var semesters = g.Select(x => new {
                        SemesterName = x.Semester.SemesterName,
                        TotalScore = x.TotalScore,
                        Rank = GetRank(x.TotalScore)
                    }).OrderBy(x => x.SemesterName).ToList();

                    // Tính trung bình cộng điểm năm
                    double avgScore = semesters.Count > 0
                        ? semesters.Where(s => s.TotalScore.HasValue).Average(s => s.TotalScore ?? 0)
                        : 0;

                    // Xếp loại năm dựa trên điểm trung bình
                    string yearRank = GetRank((int)Math.Round(avgScore));

                    return new
                    {
                        AcademicYear = g.Key,
                        Semesters = semesters,
                        TotalYearScore = Math.Round(avgScore, 2),
                        YearRank = yearRank
                    };
                })
                .ToList();

            ViewBag.GroupedSummaries = grouped;
            return View();
        }
        private string GetRank(int? totalScore)
        {
            if (totalScore == null) return "";
            if (totalScore >= 90) return "Xuất sắc";
            if (totalScore >= 80) return "Tốt";
            if (totalScore >= 65) return "Khá";
            if (totalScore >= 50) return "Trung bình";
            return "Yếu";
        }
    }
}