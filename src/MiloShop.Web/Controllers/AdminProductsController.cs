using MiloShop.Web.ViewModels.Admin;
using MiloShop.Infrastructure.Persistence;
using MiloShop.Domain.Entities;
using MiloShop.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MiloShop.Web.Controllers
{
    [Route("Admin/Products/[action]/{id?}")]
    [Authorize(Roles = "Admin")]
    [AutoValidateAntiforgeryToken]
    public class AdminProductsController : Controller
    {
        private readonly MiloDbContext _context;
        private readonly IProductImageStorage _imageStorage;

        public AdminProductsController(MiloDbContext context, IProductImageStorage imageStorage)
        {
            _context = context;
            _imageStorage = imageStorage;
        }

        [HttpGet("/Admin/Products", Name = "AdminProductsIndex")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "محصولات";

            var products = await _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "افزودن محصول";
            ViewBag.Categories = await _context.Categories.AsNoTracking().OrderByDescending(c => c.Id).ToListAsync();
            return View(new ProductUpsertVm());
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductUpsertVm vm)
        {
            ViewBag.Categories = await _context.Categories.AsNoTracking().OrderByDescending(c => c.Id).ToListAsync();

            if (vm.ImageFile == null)
                ModelState.AddModelError(nameof(vm.ImageFile), "تصویر محصول الزامی است.");

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "افزودن محصول";
                return View(vm);
            }

            var product = new Product
            {
                Title = vm.Title,
                ShortDescription = vm.ShortDescription,
                Description = vm.Description,
                CategoryId = vm.CategoryId,
                Price = vm.Price,
                SalePrice = vm.SalePrice,
                StockQuantity = vm.StockQuantity,
                Brand = vm.Brand,
                Material = vm.Material,
                IsActive = vm.IsActive,
                HomepagePlacement = vm.HomepagePlacement,
                IsSpecialOffer = vm.IsSpecialOffer,

                Slug = Slugify(vm.Title),
                Sku = $"SKU-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
                CreatedAt = DateTime.UtcNow
            };

            await using (var image = vm.ImageFile!.OpenReadStream())
            {
                product.ImageUrl = await _imageStorage.SaveAsync(
                    image,
                    Path.GetExtension(vm.ImageFile.FileName),
                    HttpContext.RequestAborted);
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var p = await _context.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return NotFound();

            ViewData["Title"] = "ویرایش محصول";
            ViewBag.Categories = await _context.Categories.AsNoTracking()
                .OrderByDescending(c => c.Id).ToListAsync();

            var vm = new ProductUpsertVm
            {
                Id = p.Id,
                Title = p.Title,
                ShortDescription = p.ShortDescription,
                Description = p.Description,
                CategoryId = p.CategoryId,
                Price = p.Price,
                SalePrice = p.SalePrice,
                StockQuantity = p.StockQuantity,
                Brand = p.Brand,
                Material = p.Material,
                IsActive = p.IsActive,
                HomepagePlacement = p.HomepagePlacement,
                IsSpecialOffer = p.IsSpecialOffer,
                ImageUrl = p.ImageUrl
            };

            return View(vm);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(int id, ProductUpsertVm vm)
        {
            if (id != vm.Id) return BadRequest();

            var existing = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (existing == null) return NotFound();

            ViewBag.Categories = await _context.Categories.AsNoTracking()
                .OrderByDescending(c => c.Id).ToListAsync();

            if (!ModelState.IsValid)
            {
                vm.ImageUrl = existing.ImageUrl;
                ViewData["Title"] = "ویرایش محصول";
                return View(vm);
            }

            existing.HomepagePlacement = vm.HomepagePlacement;
            existing.Title = vm.Title;
            existing.ShortDescription = vm.ShortDescription;
            existing.Description = vm.Description;
            existing.CategoryId = vm.CategoryId;
            existing.Price = vm.Price;
            existing.SalePrice = vm.SalePrice;
            existing.StockQuantity = vm.StockQuantity;
            existing.Brand = vm.Brand;
            existing.Material = vm.Material;
            existing.IsActive = vm.IsActive;
            existing.IsSpecialOffer = vm.IsSpecialOffer;

            var previousImage = existing.ImageUrl;
            if (vm.ImageFile != null)
            {
                await using var image = vm.ImageFile.OpenReadStream();
                existing.ImageUrl = await _imageStorage.SaveAsync(
                    image,
                    Path.GetExtension(vm.ImageFile.FileName),
                    HttpContext.RequestAborted);
                existing.MainImagePath = existing.ImageUrl;
            }

            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            if (vm.ImageFile != null) _imageStorage.DeleteIfExists(previousImage);
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _context.Products.FindAsync(id);
            if (existing == null) return NotFound();

            _imageStorage.DeleteIfExists(existing.ImageUrl);

            _context.Products.Remove(existing);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private static string Slugify(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return Guid.NewGuid().ToString("N");
            input = input.Trim().ToLowerInvariant();
            input = System.Text.RegularExpressions.Regex.Replace(input, @"\s+", "-");
            input = System.Text.RegularExpressions.Regex.Replace(input, @"[^a-z0-9\-]+", "");
            input = System.Text.RegularExpressions.Regex.Replace(input, @"-+", "-").Trim('-');
            return string.IsNullOrWhiteSpace(input) ? Guid.NewGuid().ToString("N") : input;
        }
    }
}
