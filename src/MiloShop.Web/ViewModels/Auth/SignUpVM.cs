using System.ComponentModel.DataAnnotations;

namespace MiloShop.Web.ViewModels.Auth
{
    public class SignUpVM
    {
        [Required(ErrorMessage = "username required ")]
        [StringLength(50, ErrorMessage = "Username must be less than 50 characters.")]
        public string UserName { get; set; } = string.Empty; // ✅ Correct property name

        [Required(ErrorMessage = "email required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "password required")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "reapeating pass is requred")]
        [Compare(nameof(Password), ErrorMessage = "passwords are not the same ")]
        public string ConfirmPassword { get; set; } = string.Empty;

    }
}