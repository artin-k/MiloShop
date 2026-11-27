using MiloShop.Web.Controllers;
using MiloShop.Domain.Entities;
using MiloShop.Application.Common.Interfaces;
using MiloShop.Infrastructure.Services;
using MiloShop.Tests.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Security.Claims;

namespace MiloShop.Tests.Controllers;

public class CartControllerTests
{
    [Fact]
    public async Task Add_WhenUserMissing_RedirectsToLogin()
    {
        await using var db = TestDbContextFactory.Create();
        db.Products.Add(new Product
        {
            Title = "P",
            Description = "D",
            Slug = "p",
            Sku = "sku",
            Price = 10,
            StockQuantity = 1,
            IsActive = true,
            CategoryId = 1
        });
        await db.SaveChangesAsync();

        var userManager = UserManagerMockFactory.Create();
        userManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((ApplicationUser?)null);

        var coupon = new Mock<ICouponService>();
        var sut = new CartController(db, userManager.Object, coupon.Object);

        var result = await sut.Add(1);

        var redirect = result.Should().BeOfType<RedirectResult>().Subject;
        redirect.Url.Should().Be("/Auth/Login");
    }

    [Theory]
    [InlineData(null, 2)]
    [InlineData(4, 5)]
    [InlineData(5, 1)]
    [InlineData(0, 1)]
    public async Task Add_ExistingCartItem_RespectsRequestedQuantityAndRemainingStock(int? quantity, int expectedCartQuantity)
    {
        await using var db = TestDbContextFactory.Create();
        var product = new Product
        {
            Title = "Product",
            Description = "D",
            Slug = "product",
            Sku = "sku-product",
            Price = 100,
            StockQuantity = 5,
            IsActive = true,
            CategoryId = 1
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var user = new ApplicationUser { Id = "u-1", UserName = "user" };
        db.CartItems.Add(new CartItem
        {
            UserId = user.Id,
            ProductId = product.Id,
            Quantity = 1
        });
        await db.SaveChangesAsync();

        var userManager = UserManagerMockFactory.Create();
        userManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var coupon = new Mock<ICouponService>();
        var sut = new CartController(db, userManager.Object, coupon.Object);
        var url = new Mock<IUrlHelper>();
        url.Setup(x => x.IsLocalUrl(It.IsAny<string>()))
            .Returns<string>(u => u.StartsWith("/"));
        sut.Url = url.Object;

        sut.TempData = Mock.Of<ITempDataDictionary>();
        var result = quantity.HasValue
            ? await sut.Add(product.Id, "/Shop", quantity.Value)
            : await sut.Add(product.Id, "/Shop");

        if (expectedCartQuantity == 1)
            result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Index");
        else
            result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/Shop");

        var item = db.CartItems.Single(x => x.UserId == user.Id && x.ProductId == product.Id);
        item.Quantity.Should().Be(expectedCartQuantity);
    }
}
