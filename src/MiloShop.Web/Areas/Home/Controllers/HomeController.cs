using MiloShop.Infrastructure.Persistence;
using MiloShop.Domain.Entities;
using MiloShop.Web.ViewModels;
using MiloShop.Web.Services;
using MiloShop.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MiloShop.Web.Areas.Home.Controllers
{
    [Area("Home")]
    public class HomeController : Controller
    {
        private readonly MiloDbContext _context;
        public HomeController(MiloDbContext context, ISiteSettingsService _)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var now = DateTime.UtcNow;

            var coupon = await _context.Coupons
                .AsNoTracking()
                .Where(c => c.IsActive)
                .Where(c => !c.ExpireAt.HasValue || c.ExpireAt.Value >= now)
                .Where(c => !c.UsageLimit.HasValue || c.UsedCount < c.UsageLimit.Value)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();

            var offer = await _context.Offers
                .AsNoTracking()
                .Include(o => o.Product)
                .Include(o => o.Coupon)
                .Where(o => o.IsActive)
                .Where(o => o.StartsAt <= now)
                .Where(o => o.EndsAt == null || o.EndsAt > now)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            var vm = new HomePageViewModel
            {
                Categories = await _context.Categories
                    .AsNoTracking()
                    .OrderByDescending(p => p.Id)
                    .ToListAsync(),

                NewProducts = await _context.Products
                    .AsNoTracking()
                    .Where(p => p.IsActive && p.StockQuantity > 0)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(8)
                    .ToListAsync(),

                ActiveOffer = offer,
                ActiveCoupon = coupon
            };


            return View(vm);
        }

        [HttpGet]
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
