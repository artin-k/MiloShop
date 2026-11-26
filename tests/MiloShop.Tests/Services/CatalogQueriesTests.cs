using FluentAssertions;
using MiloShop.Application.Catalog;
using MiloShop.Domain.Entities;
using MiloShop.Infrastructure.Services;
using MiloShop.Tests.Common;

namespace MiloShop.Tests.Services;

public class CatalogQueriesTests
{
    [Fact]
    public async Task Home_WithNoCategories_ReturnsAnEmptyCollectionWithoutInventingCategories()
    {
        await using var db = TestDbContextFactory.Create();
        var home = await new CatalogQueries(db).GetHomeAsync();
        home.Collections.Should().BeEmpty();
        home.Categories.Should().BeEmpty();
    }

    [Fact]
    public async Task Home_CollectionsUseStoredCategoriesAndCountsFromTheSameAvailableCatalog()
    {
        await using var db = TestDbContextFactory.Create();
        var notebook = new Category { Name = "دفتر و کاغذ", Description = "برای فکرهای تازه" };
        var empty = new Category { Name = "ابزار طراحی", Description = "دسته تازه" };
        db.Categories.AddRange(notebook, empty);
        for (var i = 0; i < 13; i++) db.Products.Add(new Product
        {
            Title = $"Notebook {i}", Slug = $"note-{i}", Sku = $"note-{i}",
            Category = notebook, IsActive = true, StockQuantity = 2
        });
        db.Products.Add(new Product { Title = "Hidden", Slug = "hidden", Sku = "hidden", Category = notebook, IsActive = false, StockQuantity = 2 });
        db.Products.Add(new Product { Title = "Sold out", Slug = "sold", Sku = "sold", Category = notebook, IsActive = true, StockQuantity = 0 });
        await db.SaveChangesAsync();
        var service = new CatalogQueries(db);
        var home = await service.GetHomeAsync();
        home.Products.Should().HaveCount(8);
        home.Collections.Should().HaveCount(2);
        var collection = home.Collections.Single(c => c.Id == notebook.Id);
        collection.Name.Should().Be(notebook.Name);
        collection.Description.Should().Be(notebook.Description);
        collection.AvailableProductCount.Should().Be(13);
        home.Collections.Single(c => c.Id == empty.Id).AvailableProductCount.Should().Be(0);
        (await service.GetCatalogAsync(new CatalogFilter(CategoryId: notebook.Id))).TotalCount.Should().Be(collection.AvailableProductCount);
        notebook.Name = "دفترهای تازه";
        await db.SaveChangesAsync();
        (await service.GetHomeAsync()).Collections.Single(c => c.Id == notebook.Id).Name.Should().Be("دفترهای تازه");
    }

    [Fact]
    public async Task HomepagePlacement_PrioritizesFeaturedAndOmitsHiddenBeforeTakingThePage()
    {
        await using var db = TestDbContextFactory.Create();
        var category = new Category { Name = "دفتر", Icon = "gift" };
        for (int i = 0; i < 12; i++)
        {
            var placement = i switch { 0 => HomepagePlacement.Featured, 11 => HomepagePlacement.Hidden, _ => HomepagePlacement.Automatic };
            var product = new Product
            {
                Category = category, Title = $"Paper {i}", Slug = $"paper-{i}", Sku = $"P{i}",
                StockQuantity = 1, HomepagePlacement = placement, CreatedAt = DateTime.UnixEpoch.AddDays(i)
            };
            db.Reviews.Add(new Review
            {
                Product = product, Comment = product.Title, Rating = 4, UserName = "Guest",
                CreatedAt = product.CreatedAt, HomepagePlacement = placement
            });
        }
        await db.SaveChangesAsync();
        var catalog = new CatalogQueries(db);
        var home = await catalog.GetHomeAsync();
        home.Products.Should().HaveCount(8);
        home.Products.First().Title.Should().Be("Paper 0");
        home.Products.Should().NotContain(p => p.Title == "Paper 11");
        home.Collections.Single().Icon.Should().Be("gift");
        var quotes = await catalog.GetTestimonialsAsync();
        quotes.Should().HaveCount(6);
        quotes.First().Comment.Should().Be("Paper 0");
        quotes.Should().NotContain(q => q.Comment == "Paper 11");
        (await catalog.GetCatalogAsync(new CatalogFilter())).TotalCount.Should().Be(12);
    }

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
