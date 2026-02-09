namespace avatCo.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;

        public DateTime OrderedAt { get; set; } = DateTime.UtcNow;

        public decimal TotalAmount { get; set; }

        // ✅ Coupon info
        public int? CouponId { get; set; }
        public Coupon? Coupon { get; set; }

        public string? CouponCode { get; set; }

        public decimal? DiscountAmount { get; set; }  // how much we discounted
        public decimal? FinalAmount { get; set; }     // total - discount

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
