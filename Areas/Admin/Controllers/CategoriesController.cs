using avatCo.Data;
using avatCo.Models;
using avatCo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace avatCo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [AutoValidateAntiforgeryToken]
    public class CategoriesController : Controller
    {
        private readonly MiloDbContext _context;
        private readonly IProductImageStorage _imageStorage;

        public CategoriesController(MiloDbContext context, IProductImageStorage imageStorage)
        {
            _context = context;
            _imageStorage = imageStorage;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "دسته‌بندی‌ها";

            var cats = await _context.Categories
                .AsNoTracking()
                .OrderByDescending(c => c.Id)
                .ToListAsync();

            return View(cats);
        }

        // ✅ CREATE (GET)
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "افزودن دسته‌بندی";
            return View(new Category());
        }

        // ✅ CREATE (POST)
        [HttpPost]
        public async Task<IActionResult> Create(Category model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                ModelState.AddModelError(nameof(model.Name), "نام الزامی است.");

            if (model.ImageFile == null)
                ModelState.AddModelError(nameof(model.ImageFile), "تصویر الزامی است.");

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "افزودن دسته‌بندی";
                return View(model);
            }

            model.ImageUrl = await _imageStorage.SaveAsync(model.ImageFile!);

            _context.Categories.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ✅ EDIT (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            ViewData["Title"] = "ویرایش دسته‌بندی";
            return View(category);
        }

        // ✅ EDIT (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Category model)
        {
            if (id != model.Id) return BadRequest();

            if (string.IsNullOrWhiteSpace(model.Name))
                ModelState.AddModelError(nameof(model.Name), "نام الزامی است.");

            var existing = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (existing == null) return NotFound();

            if (!ModelState.IsValid)
            {
                // keep existing image preview
                model.ImageUrl = existing.ImageUrl;
                ViewData["Title"] = "ویرایش دسته‌بندی";
                return View(model);
            }

            existing.Name = model.Name;
            existing.Description = model.Description;

            if (model.ImageFile != null)
            {
                _imageStorage.DeleteIfExists(existing.ImageUrl);
                existing.ImageUrl = await _imageStorage.SaveAsync(model.ImageFile);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ✅ DELETE (POST)
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _context.Categories.FindAsync(id);
            if (existing == null) return NotFound();

            _imageStorage.DeleteIfExists(existing.ImageUrl);

            _context.Categories.Remove(existing);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
