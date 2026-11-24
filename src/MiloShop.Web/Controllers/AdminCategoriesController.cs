using MiloShop.Infrastructure.Persistence;
using MiloShop.Domain.Entities;
using MiloShop.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiloShop.Web.ViewModels.Admin;

namespace MiloShop.Web.Controllers
{
    [Route("Admin/Categories/[action]/{id?}")]
    [Authorize(Roles = "Admin")]
    [AutoValidateAntiforgeryToken]
    public class AdminCategoriesController : Controller
    {
        private readonly MiloDbContext _context;
        private readonly IProductImageStorage _imageStorage;

        public AdminCategoriesController(MiloDbContext context, IProductImageStorage imageStorage)
        {
            _context = context;
            _imageStorage = imageStorage;
        }

        [HttpGet("/Admin/Categories", Name = "AdminCategoriesIndex")]
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
            return View(new CategoryUpsertVm());
        }

        // ✅ CREATE (POST)
        [HttpPost]
        public async Task<IActionResult> Create(CategoryUpsertVm model)
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

            await using var image = model.ImageFile!.OpenReadStream();
            var imageUrl = await _imageStorage.SaveAsync(
                image,
                Path.GetExtension(model.ImageFile.FileName),
                HttpContext.RequestAborted);

            _context.Categories.Add(new Category
            {
                Name = model.Name.Trim(),
                Description = model.Description?.Trim() ?? string.Empty,
                ImageUrl = imageUrl
            });
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
            return View(new CategoryUpsertVm
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ExistingImageUrl = category.ImageUrl
            });
        }

        // ✅ EDIT (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(int id, CategoryUpsertVm model)
        {
            if (id != model.Id) return BadRequest();

            if (string.IsNullOrWhiteSpace(model.Name))
                ModelState.AddModelError(nameof(model.Name), "نام الزامی است.");

            var existing = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (existing == null) return NotFound();

            if (!ModelState.IsValid)
            {
                // keep existing image preview
                model.ExistingImageUrl = existing.ImageUrl;
                ViewData["Title"] = "ویرایش دسته‌بندی";
                return View(model);
            }

            existing.Name = model.Name;
            existing.Description = model.Description?.Trim() ?? string.Empty;

            if (model.ImageFile != null)
            {
                _imageStorage.DeleteIfExists(existing.ImageUrl);
                await using var image = model.ImageFile.OpenReadStream();
                existing.ImageUrl = await _imageStorage.SaveAsync(
                    image,
                    Path.GetExtension(model.ImageFile.FileName),
                    HttpContext.RequestAborted);
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
