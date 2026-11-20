using MiloShop.Infrastructure.Persistence;
using MiloShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiloShop.Infrastructure.Services
{
    public class CouponService : ICouponService
    {
        private readonly MiloDbContext _context;

        public CouponService(MiloDbContext context)
        {
            _context = context;
        }

        public async Task<CouponCheckResult> ValidateAndCalculateAsync(
            string code,
            decimal cartTotal,
            CancellationToken cancellationToken = default)
        {
            code = (code ?? "").Trim().ToUpper();

            if (string.IsNullOrWhiteSpace(code))
                return Fail("کد تخفیف را وارد کنید.");

            if (cartTotal <= 0)
                return Fail("مبلغ سبد خرید نامعتبر است.");

            var coupon = await _context.Coupons
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Code == code, cancellationToken);

            if (coupon == null)
                return Fail("کد تخفیف یافت نشد.");

            if (!coupon.IsActive)
                return Fail("این کد غیرفعال است.");

            if (coupon.ExpireAt.HasValue && coupon.ExpireAt.Value.Date < DateTime.UtcNow.Date)
                return Fail("این کد منقضی شده است.");

            if (coupon.MinOrderAmount.HasValue && cartTotal < coupon.MinOrderAmount.Value)
                return Fail($"حداقل مبلغ سفارش برای این کد {coupon.MinOrderAmount.Value} است.");

            if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
                return Fail("سقف استفاده از این کد تکمیل شده است.");

            decimal discount = 0;

            if (coupon.Type == CouponType.Percent)
            {
                // Value = 10 => 10%
                if (coupon.Value <= 0 || coupon.Value > 100)
                    return Fail("تنظیمات کوپن نامعتبر است (درصد).");

                discount = Math.Round(cartTotal * (coupon.Value / 100m), 2);
            }
            else // Fixed
            {
                if (coupon.Value <= 0)
                    return Fail("تنظیمات کوپن نامعتبر است (مبلغ).");

                discount = coupon.Value;
            }

            // never discount more than total
            if (discount > cartTotal) discount = cartTotal;

            return new CouponCheckResult
            {
                Ok = true,
                Coupon = coupon,
                DiscountAmount = discount,
                FinalAmount = cartTotal - discount
            };
        }

        public async Task<CouponConsumeResult> ConsumeAsync(
            int couponId,
            CancellationToken cancellationToken = default)
        {
            if (!_context.Database.IsRelational())
            {
                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.Id == couponId, cancellationToken);

                if (coupon == null ||
                    !coupon.IsActive ||
                    (coupon.ExpireAt.HasValue && coupon.ExpireAt.Value < DateTime.UtcNow.Date) ||
                    (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value))
                {
                    return new CouponConsumeResult(false, "کوپن دیگر معتبر یا قابل استفاده نیست.");
                }

                coupon.UsedCount += 1;
                await _context.SaveChangesAsync(cancellationToken);
                return new CouponConsumeResult(true, null);
            }

            var today = DateTime.UtcNow.Date;
            var affectedRows = await _context.Coupons
                .Where(c => c.Id == couponId)
                .Where(c => c.IsActive)
                .Where(c => !c.ExpireAt.HasValue || c.ExpireAt.Value >= today)
                .Where(c => !c.UsageLimit.HasValue || c.UsedCount < c.UsageLimit.Value)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(c => c.UsedCount, c => c.UsedCount + 1),
                    cancellationToken);

            return affectedRows == 1
                ? new CouponConsumeResult(true, null)
                : new CouponConsumeResult(false, "کوپن دیگر معتبر یا قابل استفاده نیست.");
        }

        private CouponCheckResult Fail(string msg) => new CouponCheckResult { Ok = false, Message = msg };
    }
}
