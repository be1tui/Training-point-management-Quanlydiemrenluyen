using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QUANLYDIEMRENLUYEN.Models;
using QUANLYDIEMRENLUYEN.Utilities;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly DataContext _context;

        public AccountController(DataContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Lấy danh sách tài khoản từ cơ sở dữ liệu
            var mnList = _context.Accounts
                                 .Include(a => a.Class) // Nếu muốn hiển thị tên lớp
                                 .OrderBy(m => m.AccountId)
                                 .ToList();
            return View(mnList);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            // Tìm kiếm tài khoản theo ID và bao gồm thông tin lớp
            var account = await _context.Accounts
                                        .Include(a => a.Class)
                                        .FirstOrDefaultAsync(m => m.AccountId == id);
            if (account == null) return NotFound();

            return View(account);
        }

        public IActionResult Create()
        {
            ViewBag.ClassId = new SelectList(_context.Classes, "ClassId", "ClassName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Account mn)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem tài khoản đã tồn tại chưa
                mn.Password = !string.IsNullOrEmpty(mn.Password)
                    ? Functions.MD5Password(mn.Password)
                    : "";
                _context.Accounts.Add(mn);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ClassId = new SelectList(_context.Classes, "ClassId", "ClassName", mn.ClassId);
            return View(mn);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            // Tìm kiếm tài khoản theo ID
            var account = await _context.Accounts.FindAsync(id);
            if (account == null) return NotFound();

            ViewBag.ClassId = new SelectList(_context.Classes, "ClassId", "ClassName", account.ClassId);
            return View(account);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Account account)
        {
            if (id != account.AccountId)
                return NotFound();
            // Kiểm tra xem tài khoản có hợp lệ không
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem tài khoản có tồn tại trong cơ sở dữ liệu không
                    var existingAccount = await _context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.AccountId == id);
                    if (existingAccount == null)
                        return NotFound();

                    // Nếu mật khẩu bị thay đổi thì mã hóa lại
                    if (existingAccount.Password != account.Password)
                    {
                        account.Password = !string.IsNullOrEmpty(account.Password)
                            ? Functions.MD5Password(account.Password)
                            : existingAccount.Password;
                    }

                    _context.Update(account);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccountExists(account.AccountId))
                        return NotFound();
                    else
                        throw;
                }
            }

            // Truyền lại danh sách lớp nếu ModelState không hợp lệ
            ViewBag.ClassId = new SelectList(_context.Classes, "ClassId", "ClassName", account.ClassId);
            return View(account);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0) return NotFound();
            // Tìm kiếm tài khoản theo ID và bao gồm thông tin lớp
            var mn = await _context.Accounts
                                   .Include(a => a.Class)
                                   .FirstOrDefaultAsync(a => a.AccountId == id);
            if (mn == null) return NotFound();

            return View(mn);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            // Kiểm tra xem ID có hợp lệ không
            var delaccount = _context.Accounts.Find(id);
            if (delaccount == null) return NotFound();

            _context.Accounts.Remove(delaccount);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        private bool AccountExists(int id)
        {
            return _context.Accounts.Any(e => e.AccountId == id);
        }
    }
}
