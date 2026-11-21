using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiloShop.Infrastructure.Persistence;
using MiloShop.Domain.Entities;

namespace MiloShop.Web.Controllers
{
    [Route("Admin/Coupons/[action]/{id?}")]
    [Authorize(Roles = "Admin")]
    public class AdminCouponsController : Controller
    {
        private readonly MiloDbContext _context;

        public AdminCouponsController(MiloDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Coupons";
            var items = await _context.Coupons
                .AsNoTracking()
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return View(items);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "Create Coupon";
            return View(new Coupon());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupon model)
        {
            await ValidateCouponAsync(model, null);

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Create Coupon";
                return View(model);
            }

            model.Code = NormalizeCode(model.Code);
            _context.Coupons.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.Coupons.FindAsync(id);
            if (item == null) return NotFound();

            ViewData["Title"] = "Edit Coupon";
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Coupon model)
        {
            if (id != model.Id) return BadRequest();

            await ValidateCouponAsync(model, id);

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Edit Coupon";
                return View(model);
            }

            var item = await _context.Coupons.FindAsync(id);
            if (item == null) return NotFound();

            item.Code = NormalizeCode(model.Code);
            item.Type = model.Type;
            item.Value = model.Value;
            item.ExpireAt = model.ExpireAt;
            item.UsageLimit = model.UsageLimit;
            item.MinOrderAmount = model.MinOrderAmount;
            item.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Coupons.FindAsync(id);
            if (item == null) return NotFound();

            _context.Coupons.Remove(item);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private static string NormalizeCode(string? code)
        {
            return (code ?? string.Empty).Trim().ToUpper();
        }

        private async Task ValidateCouponAsync(Coupon model, int? existingId)
        {
            var code = NormalizeCode(model.Code);
            if (string.IsNullOrWhiteSpace(code))
            {
                ModelState.AddModelError(nameof(Coupon.Code), "Code is required.");
            }
            else
            {
                var exists = await _context.Coupons.AnyAsync(c => c.Code == code && (!existingId.HasValue || c.Id != existingId.Value));
                if (exists)
                {
                    ModelState.AddModelError(nameof(Coupon.Code), "Code already exists.");
                }
            }

            if (model.Type == CouponType.Percent)
            {
                if (model.Value <= 0 || model.Value > 100)
                {
                    ModelState.AddModelError(nameof(Coupon.Value), "Percent value must be between 0 and 100.");
                }
            }
            else
            {
                if (model.Value <= 0)
                {
                    ModelState.AddModelError(nameof(Coupon.Value), "Fixed amount must be greater than 0.");
                }
            }

            if (model.UsageLimit.HasValue && model.UsageLimit.Value < 0)
            {
                ModelState.AddModelError(nameof(Coupon.UsageLimit), "Usage limit cannot be negative.");
            }

            if (model.MinOrderAmount.HasValue && model.MinOrderAmount.Value < 0)
            {
                ModelState.AddModelError(nameof(Coupon.MinOrderAmount), "Minimum order amount cannot be negative.");
            }
        }
    }
}
