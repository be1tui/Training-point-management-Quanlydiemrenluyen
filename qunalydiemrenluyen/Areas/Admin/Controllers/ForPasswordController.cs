using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QUANLYDIEMRENLUYEN.Models;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using System;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ForPasswordController : Controller
    {
        private readonly DataContext _context;
        private readonly IConfiguration _config;

        public ForPasswordController(DataContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string email)
        {
            // Kiểm tra xem email có hợp lệ không
            var user = await _context.Accounts.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Message = "Email không tồn tại!";
                return View();
            }

            // Tạo token và lưu vào DB
            var token = Guid.NewGuid().ToString();
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiry = DateTime.Now.AddHours(30);
            await _context.SaveChangesAsync();

            // Tạo link reset
            var resetLink = Url.Action("ResetPassword", "ForPassword", new { area = "Admin", token = token }, Request.Scheme);

            // Gửi email
            try
            {
                var smtpHost = _config["Smtp:Host"];
                var smtpPort = int.Parse(_config["Smtp:Port"]);
                var smtpUser = _config["Smtp:User"];
                var smtpPass = _config["Smtp:Pass"];
                var fromEmail = _config["Smtp:From"];
                // Tạo đối tượng MailMessage
                var mail = new MailMessage();
                mail.From = new MailAddress(fromEmail);
                mail.To.Add(email);
                mail.Subject = "Đặt lại mật khẩu";
                mail.Body = $"Nhấn vào link sau để đặt lại mật khẩu: {resetLink}";
                mail.IsBodyHtml = false;
                // Tạo đối tượng SmtpClient và gửi email
                using (var smtp = new SmtpClient(smtpHost, smtpPort))
                {
                    smtp.EnableSsl = true;
                    smtp.Credentials = new NetworkCredential(smtpUser, smtpPass);
                    await smtp.SendMailAsync(mail);
                }

                ViewBag.Message = "Đã gửi email đặt lại mật khẩu. Vui lòng kiểm tra hộp thư!";
            }
            catch
            {
                ViewBag.Message = "Không gửi được email. Vui lòng thử lại sau!";
            }

            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            // Kiểm tra token có hợp lệ không
            var user = _context.Accounts.FirstOrDefault(u => u.PasswordResetToken == token && u.PasswordResetTokenExpiry > DateTime.Now);
            if (user == null)
            {
                ViewBag.Message = "Link không hợp lệ hoặc đã hết hạn!";
                return View();
            }
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string token, string newPassword, string confirmPassword)
        {
            // Kiểm tra token có hợp lệ không
            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ViewBag.Message = "Vui lòng nhập đầy đủ thông tin!";
                ViewBag.Token = token;
                return View();
            }
            // Kiểm tra mật khẩu mới và xác nhận có khớp không
            if (newPassword != confirmPassword)
            {
                ViewBag.Message = "Mật khẩu xác nhận không khớp!";
                ViewBag.Token = token;
                return View();
            }
            // Lấy người dùng theo token
            var user = await _context.Accounts.FirstOrDefaultAsync(u => u.PasswordResetToken == token && u.PasswordResetTokenExpiry > DateTime.Now);
            if (user == null)
            {
                ViewBag.Message = "Link không hợp lệ hoặc đã hết hạn!";
                return View();
            }
            // Cập nhật mật khẩu
            user.Password = QUANLYDIEMRENLUYEN.Utilities.Functions.MD5Password(newPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            await _context.SaveChangesAsync();

            ViewBag.Message = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập!";
            return RedirectToAction("Index", "Login", new { area = "Admin" });
        }
    }
}