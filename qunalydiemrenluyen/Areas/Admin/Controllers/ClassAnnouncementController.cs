using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QUANLYDIEMRENLUYEN.Areas.Admin.Models;
using QUANLYDIEMRENLUYEN.Models;
using QUANLYDIEMRENLUYEN.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ClassAnnouncementController : Controller
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public ClassAnnouncementController(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public async Task<IActionResult> Index()
        {
            // Kiểm tra quyền truy cập
            await PopulateDropdownsAsync();
            var model = new MeetingNotification { MeetingDate = DateTime.Today };
            ViewBag.Announcements = await _context.MeetingNotifications
                                        .Include(m => m.Faculty)
                                        .Include(m => m.Class)
                                        .Include(m => m.AcademicYear)
                                        .Include(m => m.Semester)
                                        .OrderByDescending(m => m.CreatedAt)
                                        .ToListAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MeetingNotification meetingNotification)
        {
            // Kiểm tra quyền truy cập
            if (ModelState.IsValid)
            {
                // Kiểm tra xem đã có thông báo họp lớp cho ngày và lớp này chưa
                try
                {
                    meetingNotification.CreatedBy = Functions.AccountId;
                    meetingNotification.CreatedAt = DateTime.Now;
                    _context.Add(meetingNotification);
                    await _context.SaveChangesAsync();

                    await SendMeetingEmailAsync(meetingNotification);
                    TempData["SuccessMessage"] = "Thông báo họp lớp đã được tạo và gửi email thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Lỗi khi tạo thông báo: {ex.Message}";
                }
            }
            // Nếu có lỗi, giữ lại thông tin đã nhập và hiển thị lại form
            await PopulateDropdownsAsync(meetingNotification);
            ViewBag.Announcements = await _context.MeetingNotifications
                                       .Include(m => m.Faculty)
                                       .Include(m => m.Class)
                                       .Include(m => m.AcademicYear)
                                       .Include(m => m.Semester)
                                       .OrderByDescending(m => m.CreatedAt)
                                       .ToListAsync();
            return View("Index", meetingNotification);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var meetingNotification = await _context.MeetingNotifications.FindAsync(id);
            if (meetingNotification == null) return NotFound();
            // Kiểm tra quyền truy cập
            await PopulateDropdownsAsync(meetingNotification);
            return View("EditClassAnnouncement", meetingNotification);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MeetingNotification meetingNotification)
        {
            // Kiểm tra xem id có khớp với thông báo đang chỉnh sửa không
            if (id != meetingNotification.MeetingId) return NotFound();
            // Kiểm tra quyền truy cập
            if (ModelState.IsValid)
            {
                // Kiểm tra xem thông báo họp lớp đã tồn tại chưa
                try
                {
                    var existingNotification = await _context.MeetingNotifications.FindAsync(id);
                    if (existingNotification == null) return NotFound();
                    // Cập nhật thông tin thông báo
                    existingNotification.FacultyId = meetingNotification.FacultyId;
                    existingNotification.ClassId = meetingNotification.ClassId;
                    existingNotification.AcademicYearId = meetingNotification.AcademicYearId;
                    existingNotification.SemesterId = meetingNotification.SemesterId;
                    existingNotification.MeetingDate = meetingNotification.MeetingDate;
                    existingNotification.MeetingTime = meetingNotification.MeetingTime;
                    existingNotification.Location = meetingNotification.Location;
                    existingNotification.Note = meetingNotification.Note;
                    existingNotification.UpdatedAt = DateTime.Now;
                    
                    _context.Update(existingNotification);
                    await _context.SaveChangesAsync();

                    await SendMeetingEmailAsync(existingNotification, isUpdate: true);
                    TempData["SuccessMessage"] = "Thông báo họp lớp đã được cập nhật và gửi email thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MeetingNotificationExists(meetingNotification.MeetingId)) return NotFound();
                    TempData["ErrorMessage"] = "Lỗi xung đột dữ liệu. Vui lòng thử lại.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Lỗi khi cập nhật thông báo: {ex.Message}";
                }
            }
            await PopulateDropdownsAsync(meetingNotification);
            return View("EditClassAnnouncement", meetingNotification);
        }

        private bool MeetingNotificationExists(int id)
        {
            return _context.MeetingNotifications.Any(e => e.MeetingId == id);
        }

        private async Task PopulateDropdownsAsync(MeetingNotification? model = null)
        {
            // Lấy danh sách khoa, lớp, năm học và học kỳ để hiển thị trong dropdown
            ViewBag.Faculties = new SelectList(await _context.Faculties.OrderBy(f => f.FacultyName).ToListAsync(), "FacultyId", "FacultyName", model?.FacultyId);
            if (model?.FacultyId != null)
            {
                ViewBag.Classes = new SelectList(await _context.Classes.Where(c => c.FacultyId == model.FacultyId).OrderBy(c => c.ClassName).ToListAsync(), "ClassId", "ClassName", model?.ClassId);
            }
            else
            {
                ViewBag.Classes = new SelectList(new List<Class>(), "ClassId", "ClassName");
            }
            ViewBag.AcademicYears = new SelectList(await _context.AcademicYears.OrderBy(ay => ay.YearName).ToListAsync(), "AcademicYearId", "YearName", model?.AcademicYearId);
            if (model?.AcademicYearId != null)
            {
                 ViewBag.Semesters = new SelectList(await _context.Semesters.Where(s => s.AcademicYearId == model.AcademicYearId).OrderBy(s => s.SemesterName).ToListAsync(), "SemesterId", "SemesterName", model?.SemesterId);
            }
            else
            {
                ViewBag.Semesters = new SelectList(new List<Semester>(), "SemesterId", "SemesterName");
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetClassesByFaculty(int facultyId)
        {
            // Lấy danh sách lớp học theo khoa
            var classes = await _context.Classes
                                    .Where(c => c.FacultyId == facultyId)
                                    .Select(c => new { c.ClassId, c.ClassName })
                                    .OrderBy(c => c.ClassName)
                                    .ToListAsync();
            return Json(classes);
        }
        [HttpGet]
        public async Task<JsonResult> GetSemestersByAcademicYear(int academicYearId)
        {
            // Lấy danh sách học kỳ theo năm học
            var semesters = await _context.Semesters
                                    .Where(s => s.AcademicYearId == academicYearId)
                                    .Select(s => new { s.SemesterId, s.SemesterName })
                                    .OrderBy(s => s.SemesterName)
                                    .ToListAsync();
            return Json(semesters);
        }

        private async Task SendMeetingEmailAsync(MeetingNotification notification, bool isUpdate = false)
        {
            // Gửi email thông báo họp lớp
            var classInfo = await _context.Classes.Include(c => c.Faculty).FirstOrDefaultAsync(c => c.ClassId == notification.ClassId);
            var academicYearInfo = await _context.AcademicYears.FindAsync(notification.AcademicYearId);
            var semesterInfo = await _context.Semesters.FindAsync(notification.SemesterId);
            // Kiểm tra thông tin lớp và khoa
            if (classInfo == null || classInfo.Faculty == null)
            {
                TempData["EmailWarningMessage"] = "Không tìm thấy thông tin lớp hoặc khoa để gửi email.";
                return;
            }
            // Lấy danh sách email của sinh viên trong lớp, bao gồm cả lớp trưởng nếu có
            var students = await _context.Accounts
                .Where(a => a.ClassId == notification.ClassId && (a.Role == "Student" || a.Role == "Classmonitor") && a.IsActive && !string.IsNullOrEmpty(a.Email))
                .Select(a => a.Email)
                .ToListAsync();
            // Nếu có lớp trưởng, thêm email của lớp trưởng vào danh sách
            if (classInfo.ClassMonitorId.HasValue)
            {
                var monitorEmail = await _context.Accounts
                    .Where(a => a.AccountId == classInfo.ClassMonitorId && a.IsActive && !string.IsNullOrEmpty(a.Email))
                    .Select(a => a.Email)
                    .FirstOrDefaultAsync();
                if (!string.IsNullOrEmpty(monitorEmail) && !students.Contains(monitorEmail!))
                {
                    students.Add(monitorEmail!);
                }
            }
            // Nếu không có sinh viên hoặc lớp trưởng nào có email hợp lệ, thông báo lỗi
            if (!students.Any())
            {
                TempData["EmailWarningMessage"] = "Không có sinh viên hoặc lớp trưởng nào có email hợp lệ trong lớp này để gửi thông báo.";
                return;
            }
            // Cấu hình SMTP để gửi email
            var smtpSettings = _configuration.GetSection("Smtp");
            var client = new SmtpClient(smtpSettings["Host"], int.Parse(smtpSettings["Port"]!))
            {
                Credentials = new NetworkCredential(smtpSettings["User"], smtpSettings["Pass"]),
                EnableSsl = true
            };
            // Tạo nội dung email
            var subject = (isUpdate ? "CẬP NHẬT: " : "") + $"Thông báo họp lớp {classInfo.ClassName} - Khoa {classInfo.Faculty.FacultyName}";
            var body = $@"
                <html><body>
                <p>Kính gửi các bạn sinh viên lớp {classInfo.ClassName},</p>
                <p>Khoa {classInfo.Faculty.FacultyName} trân trọng thông báo về buổi họp lớp {classInfo.ClassName} với các thông tin chi tiết như sau:</p>
                <ul>
                    <li><strong>Năm học:</strong> {academicYearInfo?.YearName}</li>
                    <li><strong>Học kỳ:</strong> {semesterInfo?.SemesterName}</li>
                    <li><strong>Ngày họp:</strong> {notification.MeetingDate:dd/MM/yyyy}</li>
                    <li><strong>Thời gian:</strong> {notification.MeetingTime}</li>
                    <li><strong>Địa điểm:</strong> {notification.Location}</li>
                </ul>
                {(string.IsNullOrWhiteSpace(notification.Note) ? "" : $"<p><strong>Nội dung/Ghi chú:</strong><br/>{WebUtility.HtmlEncode(notification.Note)?.Replace(Environment.NewLine, "<br/>")}</p>")}
                <p>Đề nghị các bạn sinh viên có mặt đầy đủ và đúng giờ.</p>
                <p>Trân trọng,</p>
                <p>Ban chủ nhiệm Khoa {classInfo.Faculty.FacultyName}</p>
                </body></html>";
            // Tạo đối tượng MailMessage
            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpSettings["From"]!, $"Ban chủ nhiệm Khoa {classInfo.Faculty.FacultyName}"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            // Thêm địa chỉ email của sinh viên vào danh sách người nhận
            foreach (var recipient in students.Distinct())
            {
                if(!string.IsNullOrWhiteSpace(recipient)) mailMessage.To.Add(recipient!);
            }
            // Nếu không có địa chỉ email hợp lệ nào, thông báo lỗi
            if (!mailMessage.To.Any())
            {
                 TempData["EmailWarningMessage"] = "Không có địa chỉ email hợp lệ nào để gửi thông báo.";
                 return;
            }

            try
            {
                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Email sending failed: {ex.ToString()}");
                TempData["EmailWarningMessage"] = "Thông báo đã được lưu nhưng có lỗi khi gửi email: " + ex.Message;
            }
        }
    }
}