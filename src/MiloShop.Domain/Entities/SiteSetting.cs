using System;
using System.ComponentModel.DataAnnotations;

namespace MiloShop.Domain.Entities
{
    public class SiteSetting
    {
        public int Id { get; set; }

        [Required, MaxLength(120)]
        public string Key { get; set; } = default!;   // e.g. "SupportEmail"

        [MaxLength(2000)]
        public string? Value { get; set; }            // e.g. "support@domain.com"

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
