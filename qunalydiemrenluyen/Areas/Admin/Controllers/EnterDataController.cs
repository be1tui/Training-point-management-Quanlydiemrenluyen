using Microsoft.AspNetCore.Mvc;
using QUANLYDIEMRENLUYEN.Models;
using OfficeOpenXml; // Cần cài NuGet EPPlus
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;
using QUANLYDIEMRENLUYEN.Utilities; 


namespace QUANLYDIEMRENLUYEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class EnterDataController : Controller
    {
        private readonly DataContext _context;
        public EnterDataController(DataContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        // Hiển thị form nhập dữ liệu sinh viên
        [HttpPost]
        public async Task<IActionResult> ImportStudents(IFormFile file)
        {
            // Kiểm tra xem file có được chọn không
            if (file == null || file.Length == 0)
            {
                TempData["Message"] = "Vui lòng chọn file Excel!";
                return RedirectToAction("Index");
            }
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Kiểm tra định dạng file
            using (var stream = new MemoryStream())
            { 
                await file.CopyToAsync(stream);  
                using (var package = new ExcelPackage(stream))
                {
                    // Kiểm tra xem file có chứa bảng tính không
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        TempData["Message"] = "File không hợp lệ!";
                        return RedirectToAction("Index");
                    }
                    // Kiểm tra tiêu đề cột
                    int rowCount = worksheet.Dimension.Rows;
                    for (int row = 2; row <= rowCount; row++) // Bỏ qua dòng tiêu đề
                    {
                        string username = worksheet.Cells[row, 1].Text?.Trim();
                        string fullName = worksheet.Cells[row, 2].Text?.Trim();
                        string email = worksheet.Cells[row, 3].Text?.Trim();
                        string className = worksheet.Cells[row, 4].Text?.Trim();
                        string facultyName = worksheet.Cells[row, 5].Text?.Trim();
                        string passwordRaw = worksheet.Cells[row, 6].Text?.Trim(); // Đọc mật khẩu từ cột 6
                        // Tìm hoặc tạo Faculty
                        var faculty = _context.Faculties.FirstOrDefault(f => f.FacultyName == facultyName);
                        if (faculty == null && !string.IsNullOrEmpty(facultyName))
                        {
                            faculty = new Faculty { FacultyName = facultyName };
                            _context.Faculties.Add(faculty);
                            await _context.SaveChangesAsync();
                        }

                        // Tìm hoặc tạo Class
                        var classObj = _context.Classes.FirstOrDefault(c => c.ClassName == className);
                        if (classObj == null && !string.IsNullOrEmpty(className))
                        {
                            classObj = new Class { ClassName = className, FacultyId = faculty?.FacultyId };
                            _context.Classes.Add(classObj);
                            await _context.SaveChangesAsync();
                        }

                        // Thêm sinh viên nếu chưa có
                        if (!_context.Accounts.Any(a => a.Username == username))
                        {
                            var account = new Account
                            {
                                Username = username,
                                FullName = fullName,
                                Email = email,
                                ClassId = classObj?.ClassId,
                                FacultyId = faculty?.FacultyId,
                                Role = "Student",
                                IsActive = true,
                                Password = Functions.MD5Hash(passwordRaw ?? "123456") // Mã hóa mật khẩu
                            };
                            _context.Accounts.Add(account);
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }
            TempData["Message"] = "Nhập dữ liệu thành công!";
            return RedirectToAction("Index");
        }
    }
}