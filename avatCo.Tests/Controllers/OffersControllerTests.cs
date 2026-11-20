using System.Text;
using avatCo.Areas.Admin.Controllers;
using avatCo.Areas.Admin.ViewModels;
using avatCo.Models;
using avatCo.Tests.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Moq;

namespace avatCo.Tests.Controllers;

public class OffersControllerTests
{
    [Fact]
    public async Task Edit_WithUploadedBanner_KeepsUploadedImageUrl()
    {
        await using var db = TestDbContextFactory.Create();
        var category = new Category { Name = "C1" };
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var product = new Product
        {
            Title = "P1",
            Description = "D1",
            Slug = "p1",
            Sku = "sku-p1",
            Price = 99,
            CategoryId = category.Id
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var offer = new Offer
        {
            Kind = OfferKind.Product,
            Title = "Old Offer",
            ProductId = product.Id,
            StartsAt = DateTime.UtcNow.AddDays(-1),
            BannerImageUrl = "/images/offers/old.webp"
        };
        db.Offers.Add(offer);
        await db.SaveChangesAsync();

        var tempRoot = Path.Combine(Path.GetTempPath(), $"milo-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);

        try
        {
            var env = BuildEnvironmentMock(tempRoot);
            var sut = new OffersController(db, env.Object);

            var bytes = Encoding.UTF8.GetBytes("fake-image-content");
            using var ms = new MemoryStream(bytes);
            var formFile = new FormFile(ms, 0, bytes.Length, "BannerFile", "banner.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            var vm = new OfferEditVm
            {
                Id = offer.Id,
                Kind = OfferKind.Product,
                Title = "Updated Offer",
                ProductId = product.Id,
                OfferPrice = 79,
                StartsAt = DateTime.UtcNow.AddMinutes(-10),
                EndsAt = DateTime.UtcNow.AddDays(2),
                BannerImageUrl = null,
                BannerFile = formFile
            };

            var result = await sut.Edit(offer.Id, vm);

            result.Should().BeOfType<RedirectToActionResult>()
                .Which.ActionName.Should().Be(nameof(OffersController.Index));

            var updated = await db.Offers.FindAsync(offer.Id);
            updated.Should().NotBeNull();
            updated!.BannerImageUrl.Should().NotBeNullOrWhiteSpace();
            updated.BannerImageUrl.Should().StartWith("/images/offers/");
            var savedBannerUrl = updated.BannerImageUrl!;

            var physicalPath = Path.Combine(tempRoot, savedBannerUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            File.Exists(physicalPath).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, true);
            }
        }
    }

    [Fact]
    public async Task Create_WhenOfferIsInvalid_ReturnsViewWithValidationErrors()
    {
        await using var db = TestDbContextFactory.Create();
        var env = BuildEnvironmentMock(Path.GetTempPath());
        var sut = new OffersController(db, env.Object);

        var vm = new OfferEditVm
        {
            Kind = OfferKind.Coupon,
            Title = "Coupon Offer",
            StartsAt = DateTime.UtcNow,
            CouponId = 999
        };

        var result = await sut.Create(vm);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.Model.Should().Be(vm);
        sut.ModelState.IsValid.Should().BeFalse();
    }

    private static Mock<IWebHostEnvironment> BuildEnvironmentMock(string webRootPath)
    {
        var env = new Mock<IWebHostEnvironment>();
        env.SetupGet(x => x.WebRootPath).Returns(webRootPath);
        env.SetupGet(x => x.ApplicationName).Returns("avatCo.Tests");
        env.SetupGet(x => x.EnvironmentName).Returns("Development");
        env.SetupGet(x => x.ContentRootPath).Returns(webRootPath);
        env.SetupGet(x => x.WebRootFileProvider).Returns(new PhysicalFileProvider(webRootPath));
        return env;
    }
}
