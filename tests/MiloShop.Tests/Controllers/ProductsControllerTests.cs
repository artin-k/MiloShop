using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using MiloShop.Domain.Entities;
using MiloShop.Infrastructure.Services;
using MiloShop.Tests.Common;
using MiloShop.Web.Controllers;

namespace MiloShop.Tests.Controllers;

public class ProductsControllerTests
{
    [Fact]
    public async Task Details_HidesInactiveProducts()
    {
        await using var db = TestDbContextFactory.Create();
        var product = new Product { Title = "Hidden", Slug = "hidden", Sku = "hidden", IsActive = false, Category = new Category { Name = "Stationery" } };
        db.Products.Add(product);
        await db.SaveChangesAsync();
        var controller = new ProductsController(new CatalogQueries(db));
        (await controller.Details(product.Id, product.Slug)).Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Details_RedirectsToCanonicalSlug_AndRendersOutOfStockProduct()
    {
        await using var db = TestDbContextFactory.Create();
        var product = new Product { Title = "Notebook", Slug = "notebook", Sku = "note", IsActive = true, StockQuantity = 0, Category = new Category { Name = "Stationery" } };
        db.Products.Add(product);
        await db.SaveChangesAsync();
        var controller = new ProductsController(new CatalogQueries(db));
        var redirect = (await controller.Details(product.Id, "old-name")).Should().BeOfType<RedirectToRouteResult>().Subject;
        redirect.Permanent.Should().BeTrue();
        redirect.RouteValues!["slug"].Should().Be("notebook");
        var page = (await controller.Details(product.Id, product.Slug)).Should().BeOfType<ViewResult>().Subject;
        page.Model.Should().BeOfType<Product>().Which.StockQuantity.Should().Be(0);
    }
}
