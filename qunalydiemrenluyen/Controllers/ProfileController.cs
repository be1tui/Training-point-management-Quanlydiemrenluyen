using Microsoft.AspNetCore.Mvc;
using QUANLYDIEMRENLUYEN.Models;
using QUANLYDIEMRENLUYEN.Utilities;
using QUANLYDIEMRENLUYEN.Areas.Admin.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QUANLYDIEMRENLUYEN.Controllers
{
    public class ProfileUserController : Controller
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _env;

        public ProfileUserController(DataContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            var accountId = Functions.AccountId;
            var account = _context.Accounts
                .Include(a => a.Faculty)
                .Include(a => a.Class)
                .FirstOrDefault(a => a.AccountId == accountId);
            if (account == null) return RedirectToAction("Index", "Login");
            // Lấy thông tin profile
            var profile = _context.AccountProfiles.FirstOrDefault(p => p.AccountId == accountId);
            ViewBag.Profile = profile;

            return View(account);
        }

        [HttpPost]
        public IActionResult Editprofile(int AccountId, DateTime? DateOfBirth, string? PhoneNumber, string? Gender, string? Address, string? NationalID, string? Nationality, string? Ethnicity, string? Religion)
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            var profile = _context.AccountProfiles.FirstOrDefault(p => p.AccountId == AccountId);
            if (profile == null)
            {
                // Nếu chưa có thì tạo mới
                profile = new AccountProfile
                {
                    AccountId = AccountId
                };
                _context.AccountProfiles.Add(profile);
            }
            // Cập nhật thông tin cá nhân
            profile.DateOfBirth = DateOfBirth;
            profile.PhoneNumber = PhoneNumber;
            profile.Gender = Gender;
            profile.Address = Address;
            profile.NationalID = NationalID;
            profile.Nationality = Nationality;
            profile.Ethnicity = Ethnicity;
            profile.Religion = Religion;

            _context.SaveChanges();

            TempData["Success"] = "Cập nhật thông tin cá nhân thành công!";
            return RedirectToAction("Index");
        }
    }
}