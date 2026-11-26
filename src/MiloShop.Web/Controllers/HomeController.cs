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
        private readonly ISiteSettingsService _settings;
        public HomeController(ICatalogQueries catalog, ISiteSettingsService settings)
        {
            _catalog = catalog;
            _settings = settings;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var model = await BuildPageAsync();
            model.Content = await HomepageContent.LoadAsync(_settings);
            if (model.Content["ReviewsVisibility"] == "visible")
                model.Testimonials = await _catalog.GetTestimonialsAsync();
            return View(model);
        }

        [HttpGet("Studio")]
        public async Task<IActionResult> Studio()
            => View("~/Views/Studio/Index.cshtml", await BuildPageAsync());

        private async Task<HomePageViewModel> BuildPageAsync()
        {
            var page = await _catalog.GetHomeAsync();
            return new HomePageViewModel
            {
                Categories = page.Categories,
                Collections = page.Collections,
                NewProducts = page.Products,
                ActiveOffer = page.Offer,
                ActiveCoupon = page.Coupon
            };
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet("Error")]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
