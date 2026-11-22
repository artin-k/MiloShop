using FluentAssertions;
using MiloShop.Application.Catalog;
using MiloShop.Domain.Entities;
using MiloShop.Infrastructure.Services;
using MiloShop.Tests.Common;

namespace MiloShop.Tests.Services;

public class CatalogQueriesTests
{
    [Fact]
    public async Task Catalog_PaginatesAndKeepsBrandMaterialFilters_WithStablePriceOrder()
    {
        await using var db = TestDbContextFactory.Create();
        var category = new Category { Name = "دفتر" };
        for (var i = 1; i <= 28; i++) db.Products.Add(new Product
        {
            Title = $"Notebook {i}", Slug = $"notebook-{i}", Sku = $"note-{i}",
            Category = category, IsActive = true, StockQuantity = 5, Price = 100,
            Brand = "Milo", Material = "Paper"
        });
        db.Products.Add(new Product { Title = "Other", Slug = "other", Sku = "other", Category = category,
            IsActive = true, StockQuantity = 1, Brand = "Other", Material = "Metal" });
        db.Products.Add(new Product { Title = "Hidden", Slug = "hidden", Sku = "hidden", Category = category,
            IsActive = false, StockQuantity = 1, Brand = "Private", Material = "Hidden" });
        await db.SaveChangesAsync();
        var service = new CatalogQueries(db);
        var first = await service.GetCatalogAsync(new CatalogFilter(Sort: "cheap", Brand: "Milo", Material: "Paper"));
        var second = await service.GetCatalogAsync(new CatalogFilter(Sort: "cheap", Brand: "Milo", Material: "Paper", Page: 2));
        first.TotalCount.Should().Be(28);
        first.Products.Should().HaveCount(24);
        second.Products.Should().HaveCount(4);
        second.Products.Select(p => p.Id).Should().NotIntersectWith(first.Products.Select(p => p.Id));
        second.Products.Should().OnlyContain(p => p.Brand == "Milo" && p.Material == "Paper");
        first.Brands.Should().Contain("Other").And.NotContain("Private");
        var clamped = await service.GetCatalogAsync(new CatalogFilter(Brand: "Milo", Page: int.MaxValue));
        clamped.Page.Should().Be(2);
    }
}
