using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using QUANLYDIEMRENLUYEN.Models;
using QUANLYDIEMRENLUYEN.Utilities;
using QUANLYDIEMRENLUYEN.Areas.Admin.Models;
using System.IO;
using System.Linq;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProfileController : Controller
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _env;

        public ProfileController(DataContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            var accountId = Functions.AccountId;
            var account = _context.Accounts.FirstOrDefault(a => a.AccountId == accountId);
            if (account == null) return RedirectToAction("Index", "Login");

            var profile = _context.AccountProfiles.FirstOrDefault(p => p.AccountId == accountId);
            ViewBag.Profile = profile;

            return View(account);
        }

        [HttpPost]
        public IActionResult Edit(Account model)
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (!ModelState.IsValid) return RedirectToAction("Index");

            var account = _context.Accounts.FirstOrDefault(a => a.AccountId == model.AccountId);
            if (account == null) return RedirectToAction("Index");

            account.FullName = model.FullName;
            account.Email = model.Email;
            account.Role = model.Role;
            _context.SaveChanges();

            TempData["Success"] = "Cập nhật thành công!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ChangeAvatar(IFormFile avatarFile)
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            var accountId = Functions.AccountId;
            var account = _context.Accounts.FirstOrDefault(a => a.AccountId == accountId);
            if (account == null || avatarFile == null || avatarFile.Length == 0)
                return RedirectToAction("Index");

            // Đường dẫn lưu file
            var uploadsFolder = Path.Combine(_env.WebRootPath, "Avatar");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Đặt tên file duy nhất
            var fileExt = Path.GetExtension(avatarFile.FileName);
            var fileName = $"avatar_{accountId}_{DateTime.Now.Ticks}{fileExt}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Lưu file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                avatarFile.CopyTo(stream);
            }

            // Đường dẫn truy cập từ web
            var avatarUrl = $"/Avatar/{fileName}";

            // Xóa avatar cũ nếu không phải mặc định
            if (!string.IsNullOrEmpty(account.Avatar) && !account.Avatar.Contains("default-avatar.png"))
            {
                var oldPath = Path.Combine(_env.WebRootPath, account.Avatar.TrimStart('/').Replace("/", "\\"));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            // Cập nhật DB
            account.Avatar = avatarUrl;
            _context.SaveChanges();

            // Cập nhật session
            HttpContext.Session.SetString("Avatar", avatarUrl);

            TempData["Success"] = "Đổi ảnh đại diện thành công!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult EditProfile(AccountProfile model)
        {
            var accountId = Functions.AccountId;
            var account = _context.Accounts.FirstOrDefault(a => a.AccountId == accountId);
            if (account == null) return RedirectToAction("Index");

            var profile = _context.AccountProfiles.FirstOrDefault(p => p.ProfileId == model.ProfileId && p.AccountId == accountId);


            var isAdmin = account.Role == "Admin";

            if (profile == null)
            {
                // Tạo mới nếu chưa có
                profile = new AccountProfile
                {
                    AccountId = accountId,
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    Address = model.Address,
                    Nationality = model.Nationality,
                    Ethnicity = model.Ethnicity,
                    Religion = model.Religion,
                };
                if (isAdmin)
                {
                    profile.PhoneNumber = model.PhoneNumber;
                    profile.NationalID = model.NationalID;
                }
                _context.AccountProfiles.Add(profile);
            }
            else
            {
                // Cập nhật nếu đã có
                profile.DateOfBirth = model.DateOfBirth;
                profile.Gender = model.Gender;
                profile.Address = model.Address;
                profile.Nationality = model.Nationality;
                profile.Ethnicity = model.Ethnicity;
                profile.Religion = model.Religion;
                if (isAdmin)
                {
                    profile.PhoneNumber = model.PhoneNumber;
                    profile.NationalID = model.NationalID;
                }
            }

            _context.SaveChanges();
            TempData["Success"] = "Cập nhật thông tin chung thành công!";
            return RedirectToAction("Index");
        }
    }
}