namespace MiloShop.Web.ViewModels.Profile
{
    public class OrderVM
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "";
        public decimal Total { get; set; }
    }
}