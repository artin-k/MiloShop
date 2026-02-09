using avatCo.Areas.Admin.ViewModels;
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
    [Route("Admin/[controller]/[action]")]
    [AutoValidateAntiforgeryToken]
    public class CategoriesController : Controller
    {
        private readonly MiloDbContext _context;
        private readonly IProductImageStorage _imageStorage; // reuse your existing storage

        public CategoriesController(MiloDbContext context, IProductImageStorage imageStorage)
        {
            _context = context;
            _imageStorage = imageStorage;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cats = await _context.Categories
                .AsNoTracking()
                .OrderByDescending(c => c.Id)
                .ToListAsync();

            return View(cats);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CategoryUpsertVm vm)
        {
            if (vm.ImageFile == null)
                ModelState.AddModelError(nameof(vm.ImageFile), "Image is required.");

            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelStateErrors() });

            var category = new Category
            {
                Name = vm.Name,
                Description = vm.Description,
            };

            category.ImageUrl = await _imageStorage.SaveAsync(vm.ImageFile!);

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, id = category.Id });
        }

        [HttpGet]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound(new { success = false, message = "Category not found." });

            return Json(new
            {
                id = category.Id,
                name = category.Name,
                description = category.Description,
                imageUrl = string.IsNullOrEmpty(category.ImageUrl) ? null : category.ImageUrl
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromForm] CategoryUpsertVm vm)
        {
            if (vm.Id <= 0)
                return BadRequest(new { success = false, message = "Invalid category id." });

            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelStateErrors() });

            var existing = await _context.Categories.FirstOrDefaultAsync(c => c.Id == vm.Id);
            if (existing == null)
                return NotFound(new { success = false, message = "Category not found." });

            existing.Name = vm.Name;
            existing.Description = vm.Description;

            if (vm.ImageFile != null)
            {
                _imageStorage.DeleteIfExists(existing.ImageUrl);
                existing.ImageUrl = await _imageStorage.SaveAsync(vm.ImageFile);
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _context.Categories.FindAsync(id);
            if (existing == null)
                return NotFound(new { success = false, message = "Category not found." });

            _imageStorage.DeleteIfExists(existing.ImageUrl);

            _context.Categories.Remove(existing);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        private Dictionary<string, string[]> ModelStateErrors()
        {
            return ModelState
                .Where(kvp => kvp.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
        }
    }
}
