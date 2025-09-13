using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QUANLYDIEMRENLUYEN.Models;

namespace QUANLYDIEMRENLUYEN.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        // Kiểm tra nếu chưa đăng nhập thì chuyển hướng về trang đăng nhập
        if (HttpContext.Session.GetInt32("AccountId") == null)
        {
            return RedirectToAction("Index", "Login", new { area = "Admin" });
        }
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
