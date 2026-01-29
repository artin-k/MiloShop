namespace avatCo.Areas.Profile.ViewModels
{
    public class OrderVM
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "";
        public decimal Total { get; set; }
    }
}