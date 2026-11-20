using System.Text.Json;
using avatCo.Areas.Shop;
using avatCo.Areas.Shop.ViewModels;
using avatCo.Models;
using avatCo.Tests.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace avatCo.Tests.Controllers;

public class ShopControllerTests
{
    [Fact]
    public async Task Index_AppliesFiltersAndSorting()
    {
        await using var db = TestDbContextFactory.Create();
        var categoryA = new Category { Name = "A" };
        var categoryB = new Category { Name = "B" };
        db.Categories.AddRange(categoryA, categoryB);
        await db.SaveChangesAsync();

        db.Products.AddRange(
            new Product { Title = "P1", Description = "d", Slug = "p1", Sku = "s1", Price = 100, CategoryId = categoryA.Id, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new Product { Title = "P2", Description = "d", Slug = "p2", Sku = "s2", Price = 300, SalePrice = 120, CategoryId = categoryA.Id, CreatedAt = DateTime.UtcNow },
            new Product { Title = "P3", Description = "d", Slug = "p3", Sku = "s3", Price = 90, CategoryId = categoryB.Id, CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var sut = new ShopController(db);

        var result = await sut.Index(null, "cheap", 90, 130, categoryA.Id);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        var model = view.Model.Should().BeOfType<ShopViewModel>().Subject;

        model.Products.Select(p => p.Title).Should().ContainInOrder("P1", "P2");
        model.Products.Should().OnlyContain(p => p.CategoryId == categoryA.Id);
    }

    [Fact]
    public async Task Search_ReturnsAtMostFiveItems()
    {
        await using var db = TestDbContextFactory.Create();

        for (var i = 1; i <= 7; i++)
        {
            db.Products.Add(new Product
            {
                Title = $"Toy {i}",
                Description = "toy desc",
                Slug = $"toy-{i}",
                Sku = $"sku-{i}",
                Price = 10 + i,
                CategoryId = 1
            });
        }
        await db.SaveChangesAsync();

        var sut = new ShopController(db);

        var result = await sut.Search("toy");
        var json = result.Should().BeOfType<JsonResult>().Subject;
        var items = json.Value.Should().BeAssignableTo<IEnumerable<object>>().Subject;

        items.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetProduct_WhenNotFound_ReturnsNotFound()
    {
        await using var db = TestDbContextFactory.Create();
        var sut = new ShopController(db);

        var result = await sut.GetProduct(999);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetProduct_UsesFallbackImage_WhenImageIsEmpty()
    {
        await using var db = TestDbContextFactory.Create();
        var category = new Category { Name = "Cat" };
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var product = new Product
        {
            Title = "No Image Product",
            Description = "desc",
            Slug = "no-image-product",
            Sku = "sku-no-image",
            Price = 100,
            CategoryId = category.Id,
            ImageUrl = string.Empty
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var sut = new ShopController(db);
        var url = new Mock<IUrlHelper>();
        url.Setup(x => x.Content(It.IsAny<string>())).Returns<string>(p => p.Replace("~", ""));
        sut.Url = url.Object;

        var result = await sut.GetProduct(product.Id);
        var json = result.Should().BeOfType<JsonResult>().Subject;
        var payload = JsonSerializer.SerializeToElement(json.Value);

        payload.GetProperty("imageUrl").GetString().Should().Be("/images/1.webp");
    }
}
