using avatCo.Areas.Admin.ViewModels;
using avatCo.Data;
using avatCo.Models;
using avatCo.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace avatCo.Areas.Home.Controllers
{
    [Area("Home")]
    public class HomeController : Controller
    {
        private readonly MiloDbContext _context;

        public HomeController(MiloDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
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
                        ThumbImageUrl = "/images/hero/thumb1.jpg",
                        CtaText = "مشاهده محصولات",
                        CtaUrl = "/Home/Home/Index" // change later
                    },
                    new HeroSlideVm {
                        Title = "پیشنهادهای شگفت‌انگیز",
                        Subtitle = "همین امروز خرید کن",
                        Badge = "٪۶۰ تخفیف",
                        BackgroundImageUrl = "/images/hero/hero2.jpg",
                        ThumbImageUrl = "/images/hero/thumb2.jpg",
                        CtaText = "رفتن به فروشگاه",
                        CtaUrl = "/Home/Home/Index"
                    },
                        new HeroSlideVm {
                        Title = "بهترین کیفیت در غرب کشور",
                        Subtitle = "بهترین برند ها",
                        Badge = "با پرداخت امن",
                        BackgroundImageUrl = "/images/hero/hero3.jpg",
                        ThumbImageUrl = "/images/hero/thumb3.jpg",
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
                    .OrderByDescending(p => p.CreatedAt) // if you have
                    .Take(8)
                    .ToListAsync(),

                FeaturedProducts = await _context.Products
                    .AsNoTracking()
                    .Where(p => p.IsActive) // if you have
                    .Take(8)
                    .ToListAsync()
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
