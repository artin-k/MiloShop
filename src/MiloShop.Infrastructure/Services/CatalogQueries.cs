using Microsoft.EntityFrameworkCore;
using MiloShop.Application.Catalog;
using MiloShop.Domain.Entities;
using MiloShop.Infrastructure.Persistence;

namespace MiloShop.Infrastructure.Services;

public class CatalogQueries(MiloDbContext db) : ICatalogQueries
{
    public async Task<HomePage> GetHomeAsync()
    {
        var now = DateTime.UtcNow;
        var products = await db.Products.AsNoTracking()
            .Where(p => p.IsActive && p.StockQuantity > 0)
            .OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id).Take(8).ToListAsync();
        var categories = await db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
        var coupon = await db.Coupons.AsNoTracking()
            .Where(c => c.IsActive && (!c.ExpireAt.HasValue || c.ExpireAt >= now)
                && (!c.UsageLimit.HasValue || c.UsedCount < c.UsageLimit))
            .OrderByDescending(c => c.CreatedAt).FirstOrDefaultAsync();
        var offer = await db.Offers.AsNoTracking().Include(o => o.Product).Include(o => o.Coupon)
            .Where(o => o.IsActive && o.StartsAt <= now && (!o.EndsAt.HasValue || o.EndsAt > now))
            .OrderByDescending(o => o.CreatedAt).FirstOrDefaultAsync();
        return new HomePage(products, categories, offer, coupon);
    }

    public async Task<CatalogPage> GetCatalogAsync(CatalogFilter filter)
    {
        var query = db.Products.AsNoTracking().Include(p => p.Category)
            .Where(p => p.IsActive && p.StockQuantity > 0);
        if (!string.IsNullOrWhiteSpace(filter.Query))
            query = query.Where(p => p.Title.Contains(filter.Query) || p.Description.Contains(filter.Query)
                || (p.ShortDescription != null && p.ShortDescription.Contains(filter.Query)));
        if (filter.CategoryId.HasValue) query = query.Where(p => p.CategoryId == filter.CategoryId);
        if (filter.MinPrice.HasValue) query = query.Where(p => (p.SalePrice ?? p.Price) >= filter.MinPrice);
        if (filter.MaxPrice.HasValue) query = query.Where(p => (p.SalePrice ?? p.Price) <= filter.MaxPrice);
        query = filter.Sort switch
        {
            "new" => query.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id),
            "cheap" => query.OrderBy(p => p.SalePrice ?? p.Price).ThenBy(p => p.Id),
            "expensive" => query.OrderByDescending(p => p.SalePrice ?? p.Price).ThenBy(p => p.Id),
            _ => query.OrderByDescending(p => p.Id)
        };
        return new CatalogPage(await query.ToListAsync(),
            await db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync());
    }

    public Task<Product?> GetProductAsync(int id) => db.Products.AsNoTracking()
        .Include(p => p.Category).Include(p => p.Reviews).FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

    public Task<List<Product>> SearchAsync(string term) => db.Products.AsNoTracking()
        .Where(p => p.IsActive && p.StockQuantity > 0 && (p.Title.Contains(term)
            || p.Description.Contains(term) || (p.ShortDescription != null && p.ShortDescription.Contains(term))))
        .OrderBy(p => p.Title).Take(5).ToListAsync();
}
