using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MiloShop.Web.Controllers
{
    [Route("Admin/Home/[action]/{id?}")]
    [Authorize(Roles = "Admin")]
    public class AdminHomeController : Controller
    {
        [HttpGet("/Admin")]
        [HttpGet("/Admin/Home")]
        public IActionResult Index() => View();
    }
}
