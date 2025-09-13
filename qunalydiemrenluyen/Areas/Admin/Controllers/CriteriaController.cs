using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYDIEMRENLUYEN.Models;
using QUANLYDIEMRENLUYEN.Areas.Admin.Models;
using System.Threading.Tasks;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CriteriaController : Controller
    {
        private readonly DataContext _context;
        public CriteriaController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult AddCriteria(int categoryId)
        {
            // Kiểm tra xem danh mục có tồn tại không
            ViewBag.CategoryId = categoryId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCriteria(Criteria model)
        {
            // Kiểm tra xem danh mục có tồn tại không
            if (ModelState.IsValid)
            {
                _context.Criterias.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Category");
            }
            ViewBag.CategoryId = model.CategoryId;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditCriteria(int id)
        {
            // Kiểm tra xem tiêu chí có tồn tại không
            var criteria = await _context.Criterias.FindAsync(id);
            if (criteria == null) return NotFound();
            return View(criteria);
        }

        [HttpPost]
        public async Task<IActionResult> EditCriteria(Criteria model)
        {
            // Kiểm tra xem tiêu chí có tồn tại không
            if (ModelState.IsValid)
            {
                _context.Criterias.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Category");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCriteria(int id)
        {
            // Kiểm tra xem tiêu chí có tồn tại không
            var criteria = await _context.Criterias.FindAsync(id);
            if (criteria == null) return NotFound();
            _context.Criterias.Remove(criteria);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Category");
        }
    }
}