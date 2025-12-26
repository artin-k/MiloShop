using System.ComponentModel.DataAnnotations;
using avatCo.Validation;

namespace avatCo.Areas.Admin.ViewModels
{
    public class ProductUpsertVm
    {
        public int Id { get; set; }

        [Required, StringLength(160)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(5000)]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, 1_000_000_000)]
        public decimal Price { get; set; }

        [StringLength(80)]
        public string Brand { get; set; } = string.Empty;

        [StringLength(80)]
        public string Material { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        public bool IsActive { get; set; }
        public bool IsSpecialOffer { get; set; }

        // For Create, you may require it; for Edit, optional.
        // We'll enforce it in controller based on mode.
        [AllowedImage(MaxBytes = 3 * 1024 * 1024, AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" })]
        public IFormFile? ImageFile { get; set; }

        // Helpful for edit UI (preview)
        public string? ExistingImageUrl { get; set; }
    }
}
