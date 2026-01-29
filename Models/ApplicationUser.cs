using Microsoft.AspNetCore.Identity;

namespace avatCo.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? Gender { get; set; } 
        public DateTime? DateOfBirth { get; set; }
        public string? ProfileImageUrl { get; set; }
        public DateTime? TermsAcceptedAt { get; set; }
        public string? TermsVersion { get; set; }

    }
}
