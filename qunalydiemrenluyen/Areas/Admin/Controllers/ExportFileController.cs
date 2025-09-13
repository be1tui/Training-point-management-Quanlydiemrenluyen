using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using QUANLYDIEMRENLUYEN.Models;
using QUANLYDIEMRENLUYEN.Areas.Admin.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ExportFileController : Controller
    {
        private readonly DataContext _context;
        public ExportFileController(DataContext context)
        {
            _context = context;
        }

        // Hiển thị form chọn năm, kỳ, lớp, khoa
        public async Task<IActionResult> Index(int? academicYearId, int? semesterId, int? facultyId, int? classId)
        {
            // Nếu không có giá trị nào được chọn, sử dụng giá trị mặc định
            var model = new ExportFileViewModel
            {
                AcademicYearId = academicYearId,
                SemesterId = semesterId,
                FacultyId = facultyId,
                ClassId = classId,
                AcademicYears = await _context.AcademicYears
                    .Select(a => new SelectListItem
                    {
                        Value = a.AcademicYearId.ToString(),
                        Text = a.YearName
                    }).ToListAsync(),
                Semesters = await _context.Semesters
                    .Select(s => new SelectListItem
                    {
                        Value = s.SemesterId.ToString(),
                        Text = s.SemesterName
                    }).ToListAsync(),
                Faculties = await _context.Faculties
                    .Select(f => new SelectListItem
                    {
                        Value = f.FacultyId.ToString(),
                        Text = f.FacultyName
                    }).ToListAsync(),
                Classes = await _context.Classes
                    .Select(c => new SelectListItem
                    {
                        Value = c.ClassId.ToString(),
                        Text = c.ClassName
                    }).ToListAsync()
            };

            // Lấy danh sách sinh viên theo bộ lọc
            var query = _context.EvaluationSummaries
                .Include(e => e.Account).ThenInclude(a => a.Class)
                .Include(e => e.Account).ThenInclude(a => a.Faculty)
                .Include(e => e.Semester).ThenInclude(s => s.AcademicYear)
                .AsQueryable();
            // Lọc theo các điều kiện nếu có
            if (semesterId.HasValue)
                query = query.Where(e => e.SemesterId == semesterId);
            if (classId.HasValue)
                query = query.Where(e => e.Account.ClassId == classId);
            if (facultyId.HasValue)
                query = query.Where(e => e.Account.FacultyId == facultyId);
            if (academicYearId.HasValue)
                query = query.Where(e => e.Semester.AcademicYearId == academicYearId);

            model.Students = await query.ToListAsync();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Export(ExportFileViewModel model)
        {
            // Kiểm tra xem có chọn bộ lọc không
            var query = _context.EvaluationSummaries
                .Include(e => e.Account).ThenInclude(a => a.Class)
                .Include(e => e.Account).ThenInclude(a => a.Faculty)
                .Include(e => e.Semester).ThenInclude(s => s.AcademicYear)
                .AsQueryable();
            // Lọc theo các điều kiện nếu có
            if (model.SemesterId.HasValue)
                query = query.Where(e => e.SemesterId == model.SemesterId);
            if (model.ClassId.HasValue)
                query = query.Where(e => e.Account.ClassId == model.ClassId);
            if (model.FacultyId.HasValue)
                query = query.Where(e => e.Account.FacultyId == model.FacultyId);
            if (model.AcademicYearId.HasValue)
                query = query.Where(e => e.Semester.AcademicYearId == model.AcademicYearId);
            // Lấy danh sách sinh viên theo bộ lọc
            var data = await query.ToListAsync();
            // Nếu không có dữ liệu thì trả về thông báo
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("DiemRenLuyen");
            ws.Cells[1, 1].Value = "STT";
            ws.Cells[1, 2].Value = "Mã SV";
            ws.Cells[1, 3].Value = "Họ tên";
            ws.Cells[1, 4].Value = "Lớp";
            ws.Cells[1, 5].Value = "Khoa";
            ws.Cells[1, 6].Value = "Năm học";
            ws.Cells[1, 7].Value = "Học kỳ";
            ws.Cells[1, 8].Value = "Tổng điểm";
            ws.Cells[1, 9].Value = "Xếp loại";
            // Định dạng tiêu đề
            int row = 2;
            int stt = 1;
            foreach (var item in data)
            {
                ws.Cells[row, 1].Value = stt++;
                ws.Cells[row, 2].Value = item.Account?.Username;
                ws.Cells[row, 3].Value = item.Account?.FullName;
                ws.Cells[row, 4].Value = item.Account?.Class?.ClassName;
                ws.Cells[row, 5].Value = item.Account?.Faculty?.FacultyName;
                ws.Cells[row, 6].Value = item.Semester?.AcademicYear?.YearName;
                ws.Cells[row, 7].Value = item.Semester?.SemesterName;
                ws.Cells[row, 8].Value = item.TotalScore;
                ws.Cells[row, 9].Value = item.Rank;
                row++;
            }
            // Định dạng cột
            var stream = new MemoryStream(package.GetAsByteArray());
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DiemRenLuyen.xlsx");
        }
    }
}