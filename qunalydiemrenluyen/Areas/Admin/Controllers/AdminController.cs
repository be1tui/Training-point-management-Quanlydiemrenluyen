using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters; // <- Cần thiết để dùng ActionExecutingContext
using Microsoft.AspNetCore.Http;

namespace QUANLYDIEMRENLUYEN.Areas.Admin.Controllers
{
    public class AdminController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userName = context.HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(userName))
            {
                context.Result = RedirectToAction("Index", "Login", new { area = "Admin" });
            }

            base.OnActionExecuting(context);
        }
    }
}
