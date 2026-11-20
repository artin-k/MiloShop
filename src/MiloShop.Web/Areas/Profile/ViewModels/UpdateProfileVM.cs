using System.ComponentModel.DataAnnotations;

namespace MiloShop.Web.Areas.Profile.ViewModels
{
    public class UpdateProfileVM
    {
        [Required, StringLength(50)]
        public string UserName { get; set; } = "";

        [StringLength(100)]
        public string? FullName { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public string? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }
    }
}
