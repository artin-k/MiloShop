using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using MiloShop.Domain.Entities;

namespace MiloShop.Web.Areas.Admin.ViewModels
{
    public class OfferEditVm
    {
        public int Id { get; set; }

        [Required]
        public OfferKind Kind { get; set; }

        [Required, MaxLength(120)]
        public string Title { get; set; } = "پیشنهاد شگفت‌انگیز";

        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime StartsAt { get; set; } = DateTime.UtcNow;

        public DateTime? EndsAt { get; set; }

        // ✅ stored in DB (what homepage will use)
        public string? BannerImageUrl { get; set; }

        // ✅ uploaded file from admin form
        public IFormFile? BannerFile { get; set; }

        // Product offer
        public int? ProductId { get; set; }
        public int? ProductPercentOff { get; set; }
        public decimal? OfferPrice { get; set; }

        // Coupon offer
        public int? CouponId { get; set; }
    }
}
