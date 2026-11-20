using avatCo.Models;
using avatCo.Services;
using avatCo.Tests.Common;
using FluentAssertions;

namespace avatCo.Tests.Services;

public class CouponServiceTests
{
    [Fact]
    public async Task ValidateAndCalculateAsync_PercentCoupon_AppliesExpectedDiscount()
    {
        await using var db = TestDbContextFactory.Create();
        db.Coupons.Add(new Coupon
        {
            Code = "SAVE10",
            Type = CouponType.Percent,
            Value = 10,
            IsActive = true
        });
        await db.SaveChangesAsync();

        var sut = new CouponService(db);

        var result = await sut.ValidateAndCalculateAsync("save10", 200m);

        result.Ok.Should().BeTrue();
        result.DiscountAmount.Should().Be(20m);
        result.FinalAmount.Should().Be(180m);
    }

    [Fact]
    public async Task ValidateAndCalculateAsync_ExpiredCoupon_ReturnsFail()
    {
        await using var db = TestDbContextFactory.Create();
        db.Coupons.Add(new Coupon
        {
            Code = "OLD",
            Type = CouponType.Fixed,
            Value = 20,
            IsActive = true,
            ExpireAt = DateTime.UtcNow.AddDays(-1)
        });
        await db.SaveChangesAsync();

        var sut = new CouponService(db);

        var result = await sut.ValidateAndCalculateAsync("OLD", 100m);

        result.Ok.Should().BeFalse();
        result.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ValidateAndCalculateAsync_FixedCoupon_DiscountIsCappedByCartTotal()
    {
        await using var db = TestDbContextFactory.Create();
        db.Coupons.Add(new Coupon
        {
            Code = "BIG",
            Type = CouponType.Fixed,
            Value = 500,
            IsActive = true
        });
        await db.SaveChangesAsync();

        var sut = new CouponService(db);

        var result = await sut.ValidateAndCalculateAsync("BIG", 120m);

        result.Ok.Should().BeTrue();
        result.DiscountAmount.Should().Be(120m);
        result.FinalAmount.Should().Be(0m);
    }

    [Fact]
    public async Task ConsumeAsync_ValidCoupon_IncrementsUsedCount()
    {
        await using var db = TestDbContextFactory.Create();
        var coupon = new Coupon
        {
            Code = "LIMIT1",
            Type = CouponType.Percent,
            Value = 15,
            IsActive = true,
            UsageLimit = 2,
            UsedCount = 0
        };
        db.Coupons.Add(coupon);
        await db.SaveChangesAsync();

        var sut = new CouponService(db);

        var consume = await sut.ConsumeAsync(coupon.Id);

        consume.ok.Should().BeTrue();
        var updated = await db.Coupons.FindAsync(coupon.Id);
        updated!.UsedCount.Should().Be(1);
    }
}
