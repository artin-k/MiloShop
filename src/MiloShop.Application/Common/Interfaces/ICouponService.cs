using MiloShop.Domain.Entities;

namespace MiloShop.Application.Common.Interfaces;

public interface ICouponService
{
    Task<CouponCheckResult> ValidateAndCalculateAsync(
        string code,
        decimal cartTotal,
        CancellationToken cancellationToken = default);

    Task<CouponConsumeResult> ConsumeAsync(
        int couponId,
        CancellationToken cancellationToken = default);
}

public sealed record CouponConsumeResult(bool Succeeded, string? Message);

public sealed class CouponCheckResult
{
    public bool Ok { get; init; }
    public string? Message { get; init; }
    public Coupon? Coupon { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal FinalAmount { get; init; }
}
