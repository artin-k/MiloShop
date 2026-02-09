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
    [AutoValidateAntiforgeryToken]

    public class ProductsController : Controller
    {
        private readonly MiloDbContext _context;
        private readonly IProductImageStorage _imageStorage;


        public ProductsController(MiloDbContext context, IProductImageStorage imageStorage)
        {
            _context = context;
            _imageStorage = imageStorage;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.Categories = await _context.Categories.AsNoTracking().ToListAsync();
            var products = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return View(products);
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] ProductUpsertVm vm)
        {
            // For create we require an image
            if (vm.ImageFile == null)
                ModelState.AddModelError(nameof(vm.ImageFile), "Image is required.");

            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelStateErrors() });

            var product = new Product
            {
                Title = vm.Title,
                Description = vm.Description,
                Price = vm.Price,
                Brand = vm.Brand,
                Material = vm.Material,
                CategoryId = vm.CategoryId,
                IsActive = vm.IsActive,
                IsSpecialOffer = vm.IsSpecialOffer,
                CreatedAt = DateTime.UtcNow
            };

            product.ImageUrl = await _imageStorage.SaveAsync(vm.ImageFile!);

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, id = product.Id });
        }

        [HttpGet]
        public IActionResult GetProduct(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();

            return Json(new
            {
                id = product.Id,
                isActive = product.IsActive,
                isSpecialOffer = product.IsSpecialOffer,
                title = product.Title,
                price = product.Price,
                description = product.Description,
                categoryId = product.CategoryId,
                imageUrl = string.IsNullOrEmpty(product.ImageUrl) ? null : product.ImageUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] ProductUpsertVm vm)
        {
            if (vm.Id <= 0)
                return BadRequest(new { success = false, message = "Invalid product id." });

            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelStateErrors() });

            var existing = await _context.Products.FirstOrDefaultAsync(p => p.Id == vm.Id);
            if (existing == null)
                return NotFound(new { success = false, message = "Product not found." });

            existing.Title = vm.Title;
            existing.Description = vm.Description;
            existing.Price = vm.Price;
            existing.Brand = vm.Brand;
            existing.Material = vm.Material;
            existing.CategoryId = vm.CategoryId;
            existing.IsActive = vm.IsActive;
            existing.IsSpecialOffer = vm.IsSpecialOffer;

            if (vm.ImageFile != null)
            {
                _imageStorage.DeleteIfExists(existing.ImageUrl);
                existing.ImageUrl = await _imageStorage.SaveAsync(vm.ImageFile);
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _context.Products.FindAsync(id);
            if (existing == null)
                return NotFound(new { success = false, message = "Not found" });

            _imageStorage.DeleteIfExists(existing.ImageUrl);

            _context.Products.Remove(existing);
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