using MiloShop.Web.Controllers;
using MiloShop.Domain.Entities;
using MiloShop.Web.ViewModels.Home;
using MiloShop.Infrastructure.Services;
using MiloShop.Tests.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace MiloShop.Tests.Controllers;

public class HomeControllerTests
{
    [Fact]
    public async Task Index_ReturnsViewModel_WithLatestCouponAndActiveOffer()
    {
        await using var db = TestDbContextFactory.Create();

        var category = new Category { Name = "Main" };
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var product = new Product
        {
            Title = "P",
            Description = "D",
            Slug = "p",
            Sku = "sku-p",
            Price = 100,
            CategoryId = category.Id,
            IsActive = true
        };
        db.Products.Add(product);

        var oldCoupon = new Coupon { Code = "OLD1", Type = CouponType.Percent, Value = 10, CreatedAt = DateTime.UtcNow.AddDays(-3) };
        var newCoupon = new Coupon { Code = "NEW1", Type = CouponType.Percent, Value = 20, CreatedAt = DateTime.UtcNow.AddDays(-1) };
        db.Coupons.AddRange(oldCoupon, newCoupon);

        db.Offers.AddRange(
            new Offer
            {
                Title = "Future Offer",
                Kind = OfferKind.Product,
                Product = product,
                IsActive = true,
                StartsAt = DateTime.Now.AddDays(1)
            },
            new Offer
            {
                Title = "Current Offer",
                Kind = OfferKind.Product,
                Product = product,
                IsActive = true,
                StartsAt = DateTime.Now.AddDays(-1),
                EndsAt = DateTime.Now.AddDays(2)
            });
        await db.SaveChangesAsync();

        var sut = new HomeController(new CatalogQueries(db));

        var result = await sut.Index();

        var view = result.Should().BeOfType<ViewResult>().Subject;
        var model = view.Model.Should().BeOfType<HomePageViewModel>().Subject;

        model.ActiveCoupon.Should().NotBeNull();
        model.ActiveCoupon!.Code.Should().Be("NEW1");

        model.ActiveOffer.Should().NotBeNull();
        model.ActiveOffer!.Title.Should().Be("Current Offer");
    }
}
