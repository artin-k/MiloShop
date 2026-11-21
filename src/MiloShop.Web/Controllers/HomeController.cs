using MiloShop.Application.Catalog;
using MiloShop.Web.ViewModels.Home;
using MiloShop.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace MiloShop.Web.Controllers
{

    public class HomeController : Controller
    {
        private readonly ICatalogQueries _catalog;
        public HomeController(ICatalogQueries catalog)
        {
            _catalog = catalog;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var page = await _catalog.GetHomeAsync();
            return View(new HomePageViewModel
            {
                Categories = page.Categories,
                NewProducts = page.Products,
                ActiveOffer = page.Offer,
                ActiveCoupon = page.Coupon
            });
        }

        [HttpGet("Privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet("Error")]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
