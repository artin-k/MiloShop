using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiloShop.Domain.Entities
{
    public enum OfferKind
    {
        Product = 1,  // offer is tied to a product (deal of the day)
        Coupon = 2    // offer is tied to a coupon (promo code)
    }

    public class Offer
    {
        public int Id { get; set; }

        public OfferKind Kind { get; set; }

        [Required, MaxLength(120)]
        public string Title { get; set; } = "پیشنهاد شگفت‌انگیز";

        // Countdown / active window
        public DateTime StartsAt { get; set; } = DateTime.UtcNow;
        public DateTime? EndsAt { get; set; } // null => no timer

        public bool IsActive { get; set; } = true;

        // ---------- Product Offer ----------
        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        // percent discount for product offer (optional)
        public int? ProductPercentOff { get; set; } // 60 => 60%

        // OR fixed price override (optional)
        [Column(TypeName = "decimal(18,2)")]
        public decimal? OfferPrice { get; set; }

        // ---------- Coupon Offer ----------
        public int? CouponId { get; set; }
        public Coupon? Coupon { get; set; }

        // optional banner image for the offer card
        [MaxLength(250)]
        public string? BannerImageUrl { get; set; } // "/images/offers/offer1.jpg"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
