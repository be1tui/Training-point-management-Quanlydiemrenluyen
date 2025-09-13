using Microsoft.AspNetCore.Mvc;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : AdminController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}