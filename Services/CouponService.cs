using avatCo.Data;
using avatCo.Models;
using Microsoft.EntityFrameworkCore;

namespace avatCo.Services
{
    public class CouponCheckResult
    {
        public bool Ok { get; set; }
        public string? Message { get; set; }

        public Coupon? Coupon { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
    }

    public interface ICouponService
    {
        Task<CouponCheckResult> ValidateAndCalculateAsync(string code, decimal cartTotal);
        Task<(bool ok, string? message)> ConsumeAsync(int couponId); // increment UsedCount
    }

    public class CouponService : ICouponService
    {
        private readonly MiloDbContext _context;

        public CouponService(MiloDbContext context)
        {
            _context = context;
        }

        public async Task<CouponCheckResult> ValidateAndCalculateAsync(string code, decimal cartTotal)
        {
            code = (code ?? "").Trim().ToUpper();

            if (string.IsNullOrWhiteSpace(code))
                return Fail("کد تخفیف را وارد کنید.");

            if (cartTotal <= 0)
                return Fail("مبلغ سبد خرید نامعتبر است.");

            var coupon = await _context.Coupons
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Code == code);

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

        public async Task<(bool ok, string? message)> ConsumeAsync(int couponId)
        {
            // atomic-ish increment
            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Id == couponId);
            if (coupon == null) return (false, "کوپن یافت نشد.");

            if (!coupon.IsActive) return (false, "کوپن غیرفعال است.");

            if (coupon.ExpireAt.HasValue && coupon.ExpireAt.Value.Date < DateTime.UtcNow.Date)
                return (false, "کوپن منقضی شده است.");

            if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
                return (false, "سقف استفاده از کوپن تکمیل شده است.");

            coupon.UsedCount += 1;
            await _context.SaveChangesAsync();

            return (true, null);
        }

        private CouponCheckResult Fail(string msg) => new CouponCheckResult { Ok = false, Message = msg };
    }
}
