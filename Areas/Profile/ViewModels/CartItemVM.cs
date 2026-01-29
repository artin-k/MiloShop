namespace avatCo.Areas.Profile.ViewModels
{
    public class CartItemVM
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public string ProductName { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public string? ProductImageUrl { get; set; }
    }
}