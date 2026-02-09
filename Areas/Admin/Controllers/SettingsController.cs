using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace avatCo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        public IActionResult Index() => View();
    }
}
