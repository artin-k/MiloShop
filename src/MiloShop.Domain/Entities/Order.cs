namespace MiloShop.Domain.Entities
{
    public class Order
    {
        public const string PendingReviewStatus = "در انتظار بررسی";

        public int Id { get; set; }

        public string UserId { get; set; } = null!;

        public DateTime OrderedAt { get; set; } = DateTime.UtcNow;

        public decimal TotalAmount { get; set; }

        // ✅ Coupon info
        public int? CouponId { get; set; }
        public Coupon? Coupon { get; set; }

        public string? CouponCode { get; set; }

        public decimal? DiscountAmount { get; set; }  // how much we discounted
        public decimal? FinalAmount { get; set; }     // total - discount

        public string RecipientFullName { get; set; } = string.Empty;
        public string RecipientPhoneNumber { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string? PostalCode { get; set; }
        public string? CustomerNote { get; set; }
        public string Status { get; set; } = PendingReviewStatus;

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
