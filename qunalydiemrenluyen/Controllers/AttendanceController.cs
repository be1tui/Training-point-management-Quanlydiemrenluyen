using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYDIEMRENLUYEN.Models;
using QUANLYDIEMRENLUYEN.Services;
using QUANLYDIEMRENLUYEN.Areas.Admin.Models;
using QUANLYDIEMRENLUYEN.Utilities;
using System;
using System.Linq;
using System.Collections.Generic;

namespace QUANLYDIEMRENLUYEN.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly DataContext _context;
        private static Dictionary<string, DateTime> _activeQRCodes = new();
        private static Dictionary<string, int> _qrToAttendanceId = new();
        private static Dictionary<string, HashSet<string>> _qrUsedIps = new();

        public AttendanceController(DataContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Kiểm tra quyền truy cập của lớp trưởng
            var account = _context.Accounts.Find(Functions.AccountId);
            if (account == null || account.Role != "Classmonitor" || account.ClassId == null)
            {
                TempData["ErrorMessage"] = "Tài khoản không hợp lệ hoặc không phải lớp trưởng.";
                return RedirectToAction("Index", "Home");
            }
            // Lấy danh sách buổi điểm danh của lớp trưởng
            var attendances = _context.Attendances.Include(a => a.Class)
                .Where(a => a.ClassId == account.ClassId)
                .OrderByDescending(a => a.SessionDate)
                .ToList();
            return View(attendances);
        }

        [HttpGet]
        public IActionResult ShowQR()
        {
            // Kiểm tra quyền truy cập của lớp trưởng
            var account = _context.Accounts.Include(a => a.Class).FirstOrDefault(a => a.AccountId == Functions.AccountId);
            if (account == null || account.Role != "Classmonitor" || account.ClassId == null)
                return Unauthorized("Bạn không có quyền truy cập hoặc chưa được gán lớp.");
            // Tạo buổi điểm danh mới
            var attendance = new Attendance
            {
                ClassId = account.ClassId.Value,
                SessionDate = DateTime.Now,
                CreatedBy = account.AccountId,
                CreatedAt = DateTime.Now
            };
            _context.Attendances.Add(attendance);
            _context.SaveChanges();
            // Tạo mã QR ngẫu nhiên và lưu vào bộ nhớ
            string qrData = Guid.NewGuid().ToString();
            DateTime expiry = DateTime.Now.AddSeconds(60);
            _activeQRCodes[qrData] = expiry;
            _qrToAttendanceId[qrData] = attendance.AttendanceId;
            // Tạo URL cho mã QR
            string lanIp = "172.25.74.31"; // Sửa thành IP LAN của bạn
            string host = Request.Host.Host is "localhost" or "127.0.0.1"
                ? $"{lanIp}{(Request.Host.Port.HasValue ? ":" + Request.Host.Port : "")}"
                : Request.Host.Value;
            string url = Url.Action("CheckIn", "Attendance", new { qr = qrData }, Request.Scheme, host);
            // Lưu thông tin mã QR vào ViewBag để hiển thị
            ViewBag.QRImage = QRCodeService.GenerateQRCodeUrl(url);
            ViewBag.Expiry = expiry;
            ViewBag.AttendanceId = attendance.AttendanceId;

            // Dọn QR hết hạn
            foreach (var key in _activeQRCodes.Where(x => x.Value < DateTime.Now).Select(x => x.Key).ToList())
            {
                _activeQRCodes.Remove(key);
                _qrToAttendanceId.Remove(key);
            }

            return View();
        }

        [HttpGet]
        public IActionResult CheckIn(string qr)
        {
            // Kiểm tra mã QR có hợp lệ không
            if (string.IsNullOrEmpty(qr) || !_activeQRCodes.ContainsKey(qr))
            {
                ViewBag.ErrorMessage = "Mã QR không hợp lệ hoặc đã được sử dụng.";
                return View();
            }
            // Kiểm tra thời gian hết hạn của mã QR
            if (_activeQRCodes[qr] < DateTime.Now)
            {
                ViewBag.ErrorMessage = "Mã QR đã hết hạn.";
                _activeQRCodes.Remove(qr);
                _qrToAttendanceId.Remove(qr);
                return View();
            }
            ViewBag.QRCode = qr;
            return View();
        }

        [HttpPost]
        public IActionResult CheckIn(string qr, string studentCode)
        {
            // Kiểm tra mã QR
            if (string.IsNullOrEmpty(qr) || !_activeQRCodes.ContainsKey(qr) || _activeQRCodes[qr] < DateTime.Now)
            {
                ViewBag.ErrorMessage = "Mã QR không hợp lệ hoặc đã hết hạn.";
                return View();
            }
            // Kiểm tra mã sinh viên
            if (string.IsNullOrEmpty(studentCode))
            {
                ViewBag.ErrorMessage = "Mã sinh viên không được để trống.";
                ViewBag.QRCode = qr;
                return View();
            }
            // Kiểm tra IP đã sử dụng mã QR này chưa
            string clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            _qrUsedIps.TryAdd(qr, new HashSet<string>());
            if (_qrUsedIps[qr].Contains(clientIp))
            {
                ViewBag.ErrorMessage = "Mã QR này đã được sử dụng trên thiết bị của bạn.";
                return View();
            }
            // Tìm sinh viên theo mã
            var student = _context.Accounts.FirstOrDefault(s => s.Username == studentCode && s.Role == "Student");
            if (student == null)
            {
                ViewBag.ErrorMessage = "Không tìm thấy sinh viên với mã này.";
                ViewBag.QRCode = qr;
                return View();
            }
            // Kiểm tra xem sinh viên có thuộc lớp của buổi điểm danh không
            if (!_qrToAttendanceId.TryGetValue(qr, out int attendanceId))
            {
                ViewBag.ErrorMessage = "Không tìm thấy buổi điểm danh tương ứng.";
                ViewBag.QRCode = qr;
                return View();
            }
            // Lấy buổi điểm danh
            var attendance = _context.Attendances.Find(attendanceId);
            if (attendance == null || student.ClassId != attendance.ClassId)
            {
                ViewBag.ErrorMessage = "Sinh viên không thuộc lớp của buổi điểm danh này.";
                ViewBag.QRCode = qr;
                return View();
            }
            // Kiểm tra xem sinh viên đã điểm danh chưa
            var existed = _context.AttendanceRecords.FirstOrDefault(r => r.AttendanceId == attendanceId && r.StudentId == student.AccountId);
            if (existed != null)
            {
                ViewBag.SuccessMessage = $"Sinh viên {student.FullName} đã điểm danh lúc {existed.CheckInTime:HH:mm:ss dd/MM/yyyy}.";
                return View();
            }

            _context.AttendanceRecords.Add(new AttendanceRecord
            {
                AttendanceId = attendanceId,
                StudentId = student.AccountId,
                CheckInTime = DateTime.Now
            });
            _context.SaveChanges();
            _qrUsedIps[qr].Add(clientIp);

            ViewBag.SuccessMessage = $"Sinh viên {student.FullName} điểm danh thành công!";
            return View();
        }

        [HttpGet]
        public IActionResult List(int attendanceId)
        {
            var user = _context.Accounts.Find(Functions.AccountId);
            var attendance = _context.Attendances.Include(a => a.Class).FirstOrDefault(a => a.AttendanceId == attendanceId);
            if (attendance == null)
            {
                TempData["ErrorMessage"] = "Buổi điểm danh không tồn tại.";
                return RedirectToAction("Index");
            }
            if (user == null || user.Role != "Classmonitor" || user.ClassId != attendance.ClassId)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xem danh sách này.";
                return RedirectToAction("Index");
            }
            var records = _context.AttendanceRecords.Include(r => r.Student)
                .Where(r => r.AttendanceId == attendanceId)
                .OrderBy(r => r.Student.FullName)
                .ToList();
            ViewBag.SessionDate = attendance.SessionDate;
            ViewBag.ClassName = attendance.Class?.ClassName;
            return View(records);
        }
    }
}