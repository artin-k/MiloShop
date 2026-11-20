using MiloShop.Infrastructure.Persistence;
using MiloShop.Domain.Entities;
using MiloShop.Web.Areas.Shop.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiloShop.Web.Areas.Admin.ViewModels;

namespace MiloShop.Web.Areas.Shop
{
    [Area("Shop")]
    [Route("Shop")]
    public class ShopController : Controller
    {
        private readonly MiloDbContext _context;
        public ShopController(MiloDbContext context)
        {
            _context = context;
        }

        [HttpGet("", Name = "ShopIndex")]
        public async Task<IActionResult> Index(
            string? q,
            string? sort,
            decimal? minPrice,
            decimal? maxPrice,
            int? categoryId)
        {
            var query = _context.Products
                .AsNoTracking()
                .Where(product => product.IsActive && product.StockQuantity > 0);

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(p =>
                    p.Title.Contains(q) ||
                    p.Description.Contains(q) ||
                    (p.ShortDescription != null && p.ShortDescription.Contains(q)));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => (p.SalePrice ?? p.Price) >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => (p.SalePrice ?? p.Price) <= maxPrice.Value);
            }

            query = sort switch
            {
                "new" => query.OrderByDescending(p => p.CreatedAt),
                "cheap" => query.OrderBy(p => p.SalePrice ?? p.Price),
                "expensive" => query.OrderByDescending(p => p.SalePrice ?? p.Price),
                _ => query.OrderByDescending(p => p.Id)
            };

            var model = new ShopViewModel
            {
                ShopName = "MiloShop",
                ShopDescription = "لوازم‌التحریر و هدیه برای لحظه‌های خلاقانه",
                Products = await query.ToListAsync(),
                Categories = await _context.Categories.ToListAsync(),
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

            var results = await _context.Products
                .AsNoTracking()
                .Where(product => product.IsActive && product.StockQuantity > 0)
                .Where(p =>
                    p.Title.Contains(term) ||
                    p.Description.Contains(term) ||
                    (p.ShortDescription != null && p.ShortDescription.Contains(term)))
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.ImageUrl
                })
                .Take(5) // limit results
                .ToListAsync();

            return Json(results);
        }

        [HttpGet("GetProduct/{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Reviews) // make sure you named it Reviews in Product model
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

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

