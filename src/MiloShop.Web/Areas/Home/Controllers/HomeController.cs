using MiloShop.Web.Areas.Admin.ViewModels;
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
        private readonly ISiteSettingsService _settings;
        public HomeController(MiloDbContext context, ISiteSettingsService settings)
        {
            _context = context;
            _settings = settings;
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

            var dict = await _settings.GetManyAsync(
                "SupportEmail", "MainPhone", "InstagramUrl", "WhatsappUrl", "TwitterUrl"
            );

            var vm = new HomePageViewModel
            {
                HeroTitle = "Welcome to Your Shop",
                HeroSubtitle = "You Can Buy Your Dreams Here",

                HeroSlides = new List<HeroSlideVm>
                {
                    new HeroSlideVm {
                        Title = "کالکشن ویژه زمستان",
                        Subtitle = "جدیدترین محصولات با تخفیف محدود",
                        Badge = "ویژه",
                        BackgroundImageUrl = "/images/hero/hero1.jpg",
                        ThumbImageUrl = "/images/1.webp",
                        CtaText = "مشاهده محصولات",
                        CtaUrl = "/Home/Home/Index" // change later
                    },
                    new HeroSlideVm {
                        Title = "پیشنهادهای شگفت‌انگیز",
                        Subtitle = "همین امروز خرید کن",
                        Badge = "٪۶۰ تخفیف",
                        BackgroundImageUrl = "/images/hero/hero2.jpg",
                        ThumbImageUrl = "/images/2.jpg",
                        CtaText = "رفتن به فروشگاه",
                        CtaUrl = "/Home/Home/Index"
                    },
                    new HeroSlideVm {
                        Title = "بهترین کیفیت در غرب کشور",
                        Subtitle = "بهترین برند ها",
                        Badge = "با پرداخت امن",
                        BackgroundImageUrl = "/images/hero/hero3.jpg",
                        ThumbImageUrl = "/images/3.jpg",
                        CtaText = "دیدن محصولات",
                        CtaUrl = "/Home/Home/Index"
                    }
                },

                Categories = await _context.Categories
                    .AsNoTracking()
                    .OrderByDescending(p => p.Id)
                    .ToListAsync(),

                NewProducts = await _context.Products
                    .Include(p => p.Reviews)
                    .AsNoTracking()
                    .Where(p => p.IsActive && p.StockQuantity > 0)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(8)
                    .ToListAsync(),

                FeaturedProducts = await _context.Products
                    .AsNoTracking()
                    .Where(p => p.IsActive && p.StockQuantity > 0)
                    .Take(8)
                    .ToListAsync(),

                ActiveOffer = offer,
                ActiveCoupon = coupon,

                SupportEmail = dict["SupportEmail"],
                MainPhone = dict["MainPhone"],
                InstagramUrl = dict["InstagramUrl"],
                WhatsappUrl = dict["WhatsappUrl"],
                TwitterUrl = dict["TwitterUrl"]

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
