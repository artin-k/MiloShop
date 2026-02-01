namespace avatCo.Areas.Profile.ViewModels
{
    public class DashboardVM
    {
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? FullName { get; set; }
        public string? Gender { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ProfileImageUrl { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }

        public int OrdersCount { get; set; }
        public List<CartItemVM> CartItems { get; set; } = new();
    }
}
