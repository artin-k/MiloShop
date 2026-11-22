using MiloShop.Domain.Entities;

namespace MiloShop.Application.Catalog;

public record CatalogFilter(string? Query = null, string? Sort = null,
    decimal? MinPrice = null, decimal? MaxPrice = null, int? CategoryId = null,
    string? Brand = null, string? Material = null, int Page = 1);

public record CatalogPage(List<Product> Products, List<Category> Categories)
{
    public List<string> Brands { get; init; } = [];
    public List<string> Materials { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 24;
}
public record HomePage(List<Product> Products, List<Category> Categories, Offer? Offer, Coupon? Coupon);

/// <summary>Read operations used by the storefront. Persistence belongs in Infrastructure.</summary>
public interface ICatalogQueries
{
    Task<HomePage> GetHomeAsync();
    Task<CatalogPage> GetCatalogAsync(CatalogFilter filter);
    Task<Product?> GetProductAsync(int id);
    Task<List<Product>> SearchAsync(string term);
}
