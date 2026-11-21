using MiloShop.Infrastructure.Persistence;
using MiloShop.Domain.Entities;
using MiloShop.Web.ViewModels.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiloShop.Application.Catalog;

namespace MiloShop.Web.Controllers
{

    [Route("Shop")]
    public class ShopController : Controller
    {
        private readonly MiloDbContext _context;
        private readonly ICatalogQueries _catalog;
        public ShopController(MiloDbContext context, ICatalogQueries catalog)
        {
            _context = context;
            _catalog = catalog;
        }

        [HttpGet("", Name = "ShopIndex")]
        public async Task<IActionResult> Index(
            string? q,
            string? sort,
            decimal? minPrice,
            decimal? maxPrice,
            int? categoryId)
        {
            var page = await _catalog.GetCatalogAsync(new CatalogFilter(q, sort, minPrice, maxPrice, categoryId));
            var model = new ShopViewModel
            {
                ShopName = "MiloShop",
                ShopDescription = "لوازم‌التحریر و هدیه برای لحظه‌های خلاقانه",
                Products = page.Products,
                Categories = page.Categories,
                Query = q,
                Sort = sort,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                CategoryId = categoryId
            };

            return View(model);
        }


        [HttpGet("Search")]
        public async Task<IActionResult> Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            var products = await _catalog.SearchAsync(term);
            return Json(products.Select(p => new { p.Id, p.Title, p.ImageUrl }));
        }

        [HttpGet("GetProduct/{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _catalog.GetProductAsync(id);

            if (product == null) return NotFound();

            var result = new
            {
                id = product.Id,
                title = product.Title ?? "",
                description = product.Description ?? "",
                imageUrl = string.IsNullOrEmpty(product.ImageUrl) ? Url.Content("~/images/1.webp") : product.ImageUrl,
                price = product.SalePrice ?? product.Price,
                isActive = product.IsActive,
                brand = product.Brand ?? "",
                material = product.Material ?? "",
                CategoryName = product.Category?.Name ?? "",
                Opinions = (object?)(product.Reviews?
                    .Select(r => new
                    {
                        r.UserName,
                        r.Rating,
                        r.Comment,
                        r.CreatedAt
                    })
                    .ToList()
                    .Cast<object>()
                    .ToList()) ?? new List<object>()
            };

            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOpinion([FromForm] int ProductId, [FromForm] int Rating, [FromForm] string Comment)
        {
            var product = await _context.Products.FindAsync(ProductId);
            if (product == null) return Json(new { success = false, message = "Product not found." });

            var review = new Review
            {
                ProductId = ProductId,
                UserName = "Guest", // replace with current user if auth exists
                Rating = Rating,
                Comment = Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Json(new { success = true, productId = ProductId });
        }

    }

}

