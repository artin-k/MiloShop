using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiloShop.Domain.Entities
{
    public class Product
    {
        public HomepagePlacement HomepagePlacement { get; set; }
        public int Id { get; set; }

        [Required, MaxLength(160)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? ShortDescription { get; set; }

        public string Description { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Slug { get; set; } = string.Empty;   // "galaxy-s24"

        [Required, MaxLength(80)]
        public string Sku { get; set; } = string.Empty;    // "AVT-000123"

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SalePrice { get; set; }            // null => not on sale

        public int StockQuantity { get; set; } = 0;

        public string MainImagePath { get; set; } = string.Empty;

        public string? Brand { get; set; }
        public string? Material { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsSpecialOffer { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // FK
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        // Store url
        public string ImageUrl { get; set; } = string.Empty;

    }
}
