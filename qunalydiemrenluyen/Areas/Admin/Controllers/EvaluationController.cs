using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QUANLYDIEMRENLUYEN.Areas.Admin.Models;
using QUANLYDIEMRENLUYEN.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class EvaluationConfigController : Controller
    {
        private readonly DataContext _context;

        public EvaluationConfigController(DataContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách cấu hình đánh giá đã bao gồm thông tin học kỳ và năm học, sắp xếp theo năm học và học kỳ
            var evaluationConfigs = _context.EvaluationConfigs
                                            .Include(ec => ec.Semester)
                                            .ThenInclude(s => s.AcademicYear)
                                            .OrderByDescending(ec => ec.Semester.AcademicYear.YearName)
                                            .ThenByDescending(ec => ec.Semester.SemesterName);
            return View(await evaluationConfigs.ToListAsync());
        }
        
        public IActionResult CreateConfig()
        {
            // Tạo ViewData cho danh sách học kỳ để hiển thị trong dropdown
            ViewData["SemesterId"] = new SelectList(_context.Semesters
                .Include(s => s.AcademicYear)
                .OrderBy(s => s.AcademicYear.YearName).ThenBy(s => s.SemesterName)
                .Select(s => new
                {
                    s.SemesterId,
                    DisplayText = $"{s.AcademicYear.YearName} - {s.SemesterName}"
                }), "SemesterId", "DisplayText");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateConfig([Bind("SemesterId,SelfEvalStart,SelfEvalEnd,LecturerEvalStart,LecturerEvalEnd")] EvaluationConfig evaluationConfig)
        {
            // Kiểm tra xem cấu hình đánh giá đã hợp lệ chưa
            if (ModelState.IsValid)
            {
                var existingConfig = await _context.EvaluationConfigs
                    .FirstOrDefaultAsync(ec => ec.SemesterId == evaluationConfig.SemesterId);
                // Kiểm tra xem đã có cấu hình cho học kỳ này chưa
                if (existingConfig != null)
                {
                    ModelState.AddModelError("SemesterId", "Cấu hình cho học kỳ này đã tồn tại. Vui lòng chọn học kỳ khác hoặc chỉnh sửa cấu hình hiện có.");
                }
                else if (evaluationConfig.SelfEvalStart.HasValue && evaluationConfig.SelfEvalEnd.HasValue && evaluationConfig.SelfEvalStart >= evaluationConfig.SelfEvalEnd)
                {
                    ModelState.AddModelError("SelfEvalEnd", "Thời gian kết thúc tự đánh giá phải sau thời gian bắt đầu.");
                }
                else if (evaluationConfig.LecturerEvalStart.HasValue && evaluationConfig.LecturerEvalEnd.HasValue && evaluationConfig.LecturerEvalStart >= evaluationConfig.LecturerEvalEnd)
                {
                    ModelState.AddModelError("LecturerEvalEnd", "Thời gian kết thúc giảng viên đánh giá phải sau thời gian bắt đầu.");
                }
                else
                {
                    _context.Add(evaluationConfig);
                    await _context.SaveChangesAsync();

                    // Tạo thông báo cho sinh viên
                    var semester = await _context.Semesters
                        .Include(s => s.AcademicYear)
                        .FirstOrDefaultAsync(s => s.SemesterId == evaluationConfig.SemesterId);
                    // Tạo thông báo cho tất cả sinh viên trong học kỳ này
                    string title = "Thông báo thời gian tự đánh giá rèn luyện";
                    string message = $"Thời gian tự đánh giá: {evaluationConfig.SelfEvalStart:dd/MM/yyyy HH:mm} - {evaluationConfig.SelfEvalEnd:dd/MM/yyyy HH:mm} ({semester?.AcademicYear?.YearName} - {semester?.SemesterName})";
                    // Tạo thông báo và lưu vào cơ sở dữ liệu
                    var notification = new Notification
                    {
                        Title = title,
                        Message = message,
                        CreatedAt = DateTime.Now,
                        IsRead = false,
                        AccountId = null // null để gửi cho tất cả sinh viên, hoặc lặp qua từng sinh viên nếu muốn gửi riêng
                    };
                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
            }
            // Nếu có lỗi, tạo lại ViewData cho danh sách học kỳ
            ViewData["SemesterId"] = new SelectList(_context.Semesters
                .Include(s => s.AcademicYear)
                .OrderBy(s => s.AcademicYear.YearName).ThenBy(s => s.SemesterName)
                .Select(s => new {
                    s.SemesterId,
                    DisplayText = $"{s.AcademicYear.YearName} - {s.SemesterName}"
                }), "SemesterId", "DisplayText", evaluationConfig.SemesterId);
            return View(evaluationConfig);
        }

        public async Task<IActionResult> EditConfig(int? id)
        {
            // Kiểm tra xem id có hợp lệ không
            if (id == null)
            {
                return NotFound();
            }
            // Lấy cấu hình đánh giá theo id
            var evaluationConfig = await _context.EvaluationConfigs.FindAsync(id);
            if (evaluationConfig == null)
            {
                return NotFound();
            }
            // Tạo ViewData cho danh sách học kỳ để hiển thị trong dropdown
            ViewData["SemesterId"] = new SelectList(_context.Semesters
                .Include(s => s.AcademicYear)
                .OrderBy(s => s.AcademicYear.YearName).ThenBy(s => s.SemesterName)
                .Select(s => new {
                    s.SemesterId,
                    DisplayText = $"{s.AcademicYear.YearName} - {s.SemesterName}"
                }), "SemesterId", "DisplayText", evaluationConfig.SemesterId);
            return View(evaluationConfig);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditConfig(int id, [Bind("ConfigId,SemesterId,SelfEvalStart,SelfEvalEnd,LecturerEvalStart,LecturerEvalEnd")] EvaluationConfig evaluationConfig)
        {
            if (id != evaluationConfig.ConfigId)
            {
                return NotFound();
            }
            // Kiểm tra xem cấu hình đánh giá đã hợp lệ chưa
            if (ModelState.IsValid)
            {
                if (evaluationConfig.SelfEvalStart.HasValue && evaluationConfig.SelfEvalEnd.HasValue && evaluationConfig.SelfEvalStart >= evaluationConfig.SelfEvalEnd)
                {
                    ModelState.AddModelError("SelfEvalEnd", "Thời gian kết thúc tự đánh giá phải sau thời gian bắt đầu.");
                }
                else if (evaluationConfig.LecturerEvalStart.HasValue && evaluationConfig.LecturerEvalEnd.HasValue && evaluationConfig.LecturerEvalStart >= evaluationConfig.LecturerEvalEnd)
                {
                    ModelState.AddModelError("LecturerEvalEnd", "Thời gian kết thúc giảng viên đánh giá phải sau thời gian bắt đầu.");
                }
                else
                {
                    try
                    {
                        _context.Update(evaluationConfig);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!EvaluationConfigExists(evaluationConfig.ConfigId))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewData["SemesterId"] = new SelectList(_context.Semesters
                .Include(s => s.AcademicYear)
                .OrderBy(s => s.AcademicYear.YearName).ThenBy(s => s.SemesterName)
                .Select(s => new {
                    s.SemesterId,
                    DisplayText = $"{s.AcademicYear.YearName} - {s.SemesterName}"
                }), "SemesterId", "DisplayText", evaluationConfig.SemesterId);
            return View(evaluationConfig);
        }

        [HttpPost, ActionName("DeleteConfig")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Kiểm tra xem id có hợp lệ không
            var evaluationConfig = await _context.EvaluationConfigs.FindAsync(id);
            if (evaluationConfig != null)
            {
                _context.EvaluationConfigs.Remove(evaluationConfig);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool EvaluationConfigExists(int id)
        {
            return _context.EvaluationConfigs.Any(e => e.ConfigId == id);
        }
    }
}