using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYDIEMRENLUYEN.Models;
using QUANLYDIEMRENLUYEN.Areas.Admin.Models;
using System.Threading.Tasks;
using System.Linq;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly DataContext _context;
        public CategoryController(DataContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy danh sách các danh mục tiêu chí và bao gồm thông tin tiêu chí
            var categories = await _context.CriteriaCategories
                .Include(c => c.Criterias)
                .ToListAsync();
            return View(categories);
        }

        [HttpGet]
        public IActionResult AddCategory()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(CriteriaCategory model)
        {
            if (ModelState.IsValid)
            {
                _context.CriteriaCategories.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _context.CriteriaCategories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(CriteriaCategory model)
        {
            if (ModelState.IsValid)
            {
                _context.CriteriaCategories.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.CriteriaCategories
                .Include(c => c.Criterias)
                .FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null) return NotFound();

            if (category.Criterias != null)
                _context.Criterias.RemoveRange(category.Criterias);

            _context.CriteriaCategories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}