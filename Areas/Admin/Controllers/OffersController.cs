using avatCo.Areas.Admin.ViewModels;
using avatCo.Data;
using avatCo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace avatCo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OffersController : Controller
    {
        private readonly MiloDbContext _context;
        private readonly IWebHostEnvironment _env;

        public OffersController(MiloDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Offers";

            var items = await _context.Offers
                .AsNoTracking()
                .Include(o => o.Product)
                .Include(o => o.Coupon)
                .OrderByDescending(o => o.Id)
                .ToListAsync();

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Create Offer";
            await FillLookups();

            // Default: Coupon offer (you can change)
            var vm = new OfferEditVm
            {
                Kind = OfferKind.Coupon,
                StartsAt = DateTime.UtcNow
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OfferEditVm vm)
        {
            await ValidateOffer(vm);

            var uploadedUrl = await SaveOfferBannerAsync(vm.BannerFile);
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Create Offer";
                await FillLookups();
                return View(vm);
            }

            var bannerUrl = uploadedUrl ?? (string.IsNullOrWhiteSpace(vm.BannerImageUrl) ? null : vm.BannerImageUrl.Trim());

            var offer = new Offer
            {
                Kind = vm.Kind,
                Title = vm.Title.Trim(),
                IsActive = vm.IsActive,
                StartsAt = vm.StartsAt,
                EndsAt = vm.EndsAt,
                BannerImageUrl = bannerUrl,

                ProductId = vm.Kind == OfferKind.Product ? vm.ProductId : null,
                ProductPercentOff = vm.Kind == OfferKind.Product ? vm.ProductPercentOff : null,
                OfferPrice = vm.Kind == OfferKind.Product ? vm.OfferPrice : null,

                CouponId = vm.Kind == OfferKind.Coupon ? vm.CouponId : null,
            };

            _context.Offers.Add(offer);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var offer = await _context.Offers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (offer == null) return NotFound();

            ViewData["Title"] = "Edit Offer";
            await FillLookups();

            var vm = new OfferEditVm
            {
                Id = offer.Id,
                Kind = offer.Kind,
                Title = offer.Title,
                IsActive = offer.IsActive,
                StartsAt = offer.StartsAt,
                EndsAt = offer.EndsAt,
                BannerImageUrl = offer.BannerImageUrl,

                ProductId = offer.ProductId,
                ProductPercentOff = offer.ProductPercentOff,
                OfferPrice = offer.OfferPrice,

                CouponId = offer.CouponId
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OfferEditVm vm)
        {
            if (id != vm.Id) return BadRequest();

            await ValidateOffer(vm);

            var offer = await _context.Offers.FirstOrDefaultAsync(x => x.Id == id);
            if (offer == null) return NotFound();

            var uploadedUrl = await SaveOfferBannerAsync(vm.BannerFile);
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Edit Offer";
                await FillLookups();
                return View(vm);
            }

            var finalBannerImageUrl = uploadedUrl;
            if (string.IsNullOrWhiteSpace(finalBannerImageUrl))
            {
                finalBannerImageUrl = string.IsNullOrWhiteSpace(vm.BannerImageUrl)
                    ? offer.BannerImageUrl
                    : vm.BannerImageUrl.Trim();
            }

            offer.Kind = vm.Kind;
            offer.Title = vm.Title.Trim();
            offer.IsActive = vm.IsActive;
            offer.StartsAt = vm.StartsAt;
            offer.EndsAt = vm.EndsAt;
            offer.BannerImageUrl = finalBannerImageUrl;

            if (vm.Kind == OfferKind.Product)
            {
                offer.ProductId = vm.ProductId;
                offer.ProductPercentOff = vm.ProductPercentOff;
                offer.OfferPrice = vm.OfferPrice;
                offer.CouponId = null;
            }
            else
            {
                offer.CouponId = vm.CouponId;
                offer.ProductId = null;
                offer.ProductPercentOff = null;
                offer.OfferPrice = null;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var offer = await _context.Offers.FindAsync(id);
            if (offer == null) return NotFound();

            _context.Offers.Remove(offer);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task FillLookups()
        {
            var products = await _context.Products
                .AsNoTracking()
                .OrderByDescending(p => p.Id)
                .Select(p => new { p.Id, p.Title })
                .Take(200)
                .ToListAsync();

            var coupons = await _context.Coupons
                .AsNoTracking()
                .OrderByDescending(c => c.Id)
                .Select(c => new { c.Id, c.Code })
                .Take(200)
                .ToListAsync();

            ViewBag.Products = new SelectList(products, "Id", "Title");
            ViewBag.Coupons = new SelectList(coupons, "Id", "Code");
        }

        private async Task ValidateOffer(OfferEditVm vm)
        {
            // Title
            if (string.IsNullOrWhiteSpace(vm.Title))
                ModelState.AddModelError(nameof(vm.Title), "Title is required.");

            // Times
            if (vm.EndsAt.HasValue && vm.EndsAt.Value <= vm.StartsAt)
                ModelState.AddModelError(nameof(vm.EndsAt), "EndsAt must be after StartsAt.");

            // Kind rules
            if (vm.Kind == OfferKind.Product)
            {
                if (!vm.ProductId.HasValue)
                    ModelState.AddModelError(nameof(vm.ProductId), "Product is required for Product offer.");

                if (vm.ProductPercentOff.HasValue && (vm.ProductPercentOff < 1 || vm.ProductPercentOff > 100))
                    ModelState.AddModelError(nameof(vm.ProductPercentOff), "Percent off must be 1..100.");

                if (vm.OfferPrice.HasValue && vm.OfferPrice <= 0)
                    ModelState.AddModelError(nameof(vm.OfferPrice), "Offer price must be > 0.");

                if (vm.ProductPercentOff.HasValue && vm.OfferPrice.HasValue)
                    ModelState.AddModelError(nameof(vm.OfferPrice), "Use either PercentOff OR OfferPrice, not both.");
            }
            else
            {
                if (!vm.CouponId.HasValue)
                    ModelState.AddModelError(nameof(vm.CouponId), "Coupon is required for Coupon offer.");

                // optional: ensure coupon exists + active
                var couponExists = await _context.Coupons.AnyAsync(c => c.Id == vm.CouponId);
                if (!couponExists)
                    ModelState.AddModelError(nameof(vm.CouponId), "Selected coupon does not exist.");
            }
        }

        private async Task<string?> SaveOfferBannerAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            // basic validation
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowed.Contains(ext))
            {
                ModelState.AddModelError("BannerFile", "Only jpg, jpeg, png, webp are allowed.");
                return null;
            }

            const long maxBytes = 2 * 1024 * 1024; // 2MB
            if (file.Length > maxBytes)
            {
                ModelState.AddModelError("BannerFile", "Max file size is 2MB.");
                return null;
            }

            var folder = Path.Combine(_env.WebRootPath, "images", "offers");
            Directory.CreateDirectory(folder);

            var safeName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(folder, safeName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // return public url
            return $"/images/offers/{safeName}";
        }

    }
}
