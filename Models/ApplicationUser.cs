using Microsoft.AspNetCore.Identity;

namespace avatCo.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
