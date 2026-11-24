using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MiloShop.Web.Controllers
{
    [Route("Admin/Home/[action]/{id?}")]
    [Authorize(Roles = "Admin")]
    public class AdminHomeController : Controller
    {
        [HttpGet("/Admin", Name = "AdminDashboard")]
        [HttpGet("/Admin/Home")]
        [HttpGet]
        public IActionResult Index() => View();
    }
}
