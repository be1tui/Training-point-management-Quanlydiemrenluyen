using QUANLYDIEMRENLUYEN.Areas.Admin.Models;
using QUANLYDIEMRENLUYEN.Models;
using QUANLYDIEMRENLUYEN.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LoginController : Controller
    {
        private readonly DataContext _context;

        public LoginController(DataContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(Account user)
        {
            // Kiểm tra xem người dùng đã nhập tên đăng nhập chưa
            if (user == null) return NotFound();
            string inputPassword = user.Password ?? string.Empty;
            string pw = Functions.MD5Password(user.Password ?? string.Empty);

            Account? check = null;

            // Nếu nhập đúng định dạng ngày sinh dd/MM/yyyy
            if (DateTime.TryParseExact(inputPassword, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime dob))
            {
                // Khi tạo mật khẩu mặc định là ngày sinh
                string dobPassword = dob.ToString("dd/MM/yyyy");
                string dobPwHash = Functions.MD5Password(dobPassword);

                check = _context.Accounts
                    .Where(u => u.Username == user.Username && u.Password == dobPwHash && u.IsActive)
                    .FirstOrDefault();
            }
            // Nếu nhập mật khẩu có 4 ký tự trở lên (mật khẩu đã đổi)
            else if (inputPassword.Length >= 4)
            {
                check = _context.Accounts
                    .Where(u => u.Username == user.Username && u.Password == pw && u.IsActive)
                    .FirstOrDefault();
            }

            // var check = _context.Accounts
            //     .Where(u => u.Username == user.Username && u.Password == pw && u.IsActive)
            //     .FirstOrDefault();

            if (check == null)
            {
                Functions.Message = "Tên người dùng hoặc mật khẩu không hợp lệ!";
                return RedirectToAction("Index", "Login");
            }

            Functions.Message = string.Empty;
            HttpContext.Session.SetInt32("AccountId", check.AccountId);
            HttpContext.Session.SetString("Username", check.Username ?? string.Empty);
            HttpContext.Session.SetString("Email", check.Email ?? string.Empty);
            HttpContext.Session.SetString("FullName", check.FullName ?? string.Empty);
            HttpContext.Session.SetString("Avatar", string.IsNullOrEmpty(check.Avatar) ? Functions.AvatarDefault() : check.Avatar);
            HttpContext.Session.SetString("Role", check.Role ?? "User");
            HttpContext.Session.SetInt32("IsActive", check.IsActive ? 1 : 0);
            HttpContext.Session.SetString("CreatedAt", check.CreatedAt.ToString());

           if (check.Role == "Lecturer" || check.Role == "Admin")
            {
                return RedirectToAction("Index", "Home");
            }
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }
    }
}
