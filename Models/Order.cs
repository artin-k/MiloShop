namespace avatCo.Models
{
    public class Order
    {
        public int Id { get; set; }

        // Identity user FK (IdentityUser.Id is string)
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;

        // Prefer UtcNow
        public DateTime OrderedAt { get; set; } = DateTime.UtcNow;

        public decimal TotalAmount { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
