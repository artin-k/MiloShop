using System.Data;

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
        public DateTime? RegisteredAt { get; set; }
        public string? Address { get; set; }

        //  use lists, not string
        public List<OrderVM> Orders { get; set; } = new();
        public List<CartItemVM> CartItems { get; set; } = new();
    }
}