using System.ComponentModel.DataAnnotations;

namespace avatCo.Areas.Profile.ViewModels
{
    public class UpdateProfileVM
    {
        [Required, StringLength(50)]
        public string UserName { get; set; } = "";

        [StringLength(100)]
        public string? FullName { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(20)]
        public string? Gender { get; set; } // "Male" / "Female" / "Other" / null

        public DateTime? DateOfBirth { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }
    }
}