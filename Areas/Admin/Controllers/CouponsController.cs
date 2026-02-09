using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using avatCo.Data;
using avatCo.Models;

namespace avatCo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CouponsController : Controller
    {
        private readonly MiloDbContext _context;

        public CouponsController(MiloDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "کوپن‌های تخفیف";
            var items = await _context.Coupons
                .AsNoTracking()
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return View(items);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "افزودن کوپن تخفیف جدید";
            return View(new Coupon());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupon model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "افزودن کوپن تخفیف جدید";
                return View(model);
            }

            _context.Coupons.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.Coupons.FindAsync(id);
            if (item == null) return NotFound();

            ViewData["Title"] = "ویرایش کوپن";
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Coupon model)
        {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "ویرایش کوپن";
                return View(model);
            }

            _context.Coupons.Update(model);
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
    }
}
