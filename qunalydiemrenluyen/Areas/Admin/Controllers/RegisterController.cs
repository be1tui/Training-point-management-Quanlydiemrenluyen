using QUANLYDIEMRENLUYEN.Areas.Admin.Models;
using QUANLYDIEMRENLUYEN.Models;
using QUANLYDIEMRENLUYEN.Utilities;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RegisterController : Controller
    {
        private readonly DataContext _context;

        public RegisterController(DataContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(Account auser)
        {
            if (auser == null)
                return NotFound();

            // Lấy Role từ Form
            string selectedRole = Request.Form["Role"].ToString();
            if (string.IsNullOrEmpty(selectedRole))
            {
                TempData["Message"] = "Vui lòng chọn loại tài khoản.";
                return RedirectToAction("Index");
            }

            // Lấy xác nhận mật khẩu
            string confirmPassword = Request.Form["ConfirmPassword"].ToString();
            if (auser.Password != confirmPassword)
            {
                TempData["Message"] = "Mật khẩu và xác nhận mật khẩu không khớp.";
                return RedirectToAction("Index");
            }

            // Lấy Fullname từ Form
            string fullname = Request.Form["FullName"].ToString();
            if (string.IsNullOrEmpty(fullname))
            {
                TempData["Message"] = "Vui lòng nhập họ và tên.";
                return RedirectToAction("Index");
            }
            auser.FullName = fullname;

            // Gán role vào đối tượng người dùng
            auser.Role = selectedRole;

            // Kiểm tra trùng lặp và validate
            bool isDuplicate = false;
            string duplicateMessage = "";

            // Kiểm tra trùng lặp và validate theo loại tài khoản
            if (selectedRole == "Student" || selectedRole == "Classmonitor")
            {
                if (string.IsNullOrEmpty(auser.Username))
                {
                    TempData["Message"] = "Vui lòng nhập mã sinh viên.";
                    return RedirectToAction("Index");
                }
                // Kiểm tra trùng mã sinh viên
                var checkUsername = _context.Accounts.FirstOrDefault(u => u.Username == auser.Username);
                if (checkUsername != null)
                {
                    isDuplicate = true;
                    duplicateMessage = "Mã sinh viên đã tồn tại!";
                }
                // Email bắt buộc kiểm tra trùng
                auser.Email = Request.Form["Email"];
                if (string.IsNullOrEmpty(auser.Email))
                {
                    TempData["Message"] = "Vui lòng nhập email.";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(auser.Email))
                {
                    TempData["Message"] = "Vui lòng nhập email.";
                    return RedirectToAction("Index");
                }
                // Kiểm tra trùng email
                var checkEmail = _context.Accounts.FirstOrDefault(u => u.Email == auser.Email);
                if (checkEmail != null)
                {
                    isDuplicate = true;
                    duplicateMessage = "Email đã tồn tại!";
                }
                // Đảm bảo Username không bị null (có thể dùng Email làm Username nếu muốn)
                if (string.IsNullOrEmpty(auser.Username))
                {
                    auser.Username = auser.Email;
                }
                // Kiểm tra trùng username (nếu có)
                var checkUsername = _context.Accounts.FirstOrDefault(u => u.Username == auser.Username);
                if (checkUsername != null)
                {
                    isDuplicate = true;
                    duplicateMessage = "Username đã tồn tại!";
                }
            }


            // Nếu có bất kỳ trùng lặp nào thì trả về lỗi
            if (isDuplicate)
            {
                TempData["Message"] = duplicateMessage;
                return RedirectToAction("Index");
            }

            // Đăng ký tài khoản
            auser.Password = Functions.MD5Password(auser.Password);
            auser.CreatedAt = DateTime.Now;
            auser.IsActive = true;

            _context.Accounts.Add(auser);
            _context.SaveChanges();

            TempData["Message"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Index", "Register");
        }
    }
}
