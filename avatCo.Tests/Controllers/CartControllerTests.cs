using avatCo.Controllers;
using avatCo.Models;
using avatCo.Services;
using avatCo.Tests.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace avatCo.Tests.Controllers;

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

    [Fact]
    public async Task Add_ExistingCartItem_IncrementsQuantity_AndRedirectsToLocalReturnUrl()
    {
        await using var db = TestDbContextFactory.Create();
        var product = new Product
        {
            Title = "Product",
            Description = "D",
            Slug = "product",
            Sku = "sku-product",
            Price = 100,
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

        var result = await sut.Add(product.Id, "/Shop");

        var redirect = result.Should().BeOfType<RedirectResult>().Subject;
        redirect.Url.Should().Be("/Shop");

        var item = db.CartItems.Single(x => x.UserId == user.Id && x.ProductId == product.Id);
        item.Quantity.Should().Be(2);
    }
}
