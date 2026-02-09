using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace avatCo.Areas.Admin.ViewModels
{
    public class ProductUpsertVm
    {
        public int Id { get; set; }

        [Required, MaxLength(160)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? ShortDescription { get; set; }

        public string Description { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        public decimal? SalePrice { get; set; }

        public int StockQuantity { get; set; }

        public string? Brand { get; set; }
        public string? Material { get; set; }

        public bool IsActive { get; set; }
        public bool IsSpecialOffer { get; set; }

        public string? ImageUrl { get; set; } // for edit preview
        public IFormFile? ImageFile { get; set; } // upload
    }
}
