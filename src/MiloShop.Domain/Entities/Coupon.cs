using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiloShop.Domain.Entities
{
    public enum CouponType
    {
        Percent = 1,   // e.g. 10%
        Fixed = 2      // e.g. 50000 toman
    }

    public class Coupon
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Code { get; set; } = string.Empty;  // "SPRING2024"

        public CouponType Type { get; set; } = CouponType.Percent;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Value { get; set; } // 10 => 10% OR 50000 => fixed amount

        public DateTime? ExpireAt { get; set; } // null => no expiry

        public int? UsageLimit { get; set; } // null => unlimited
        public int UsedCount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MinOrderAmount { get; set; } // null => no minimum

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
