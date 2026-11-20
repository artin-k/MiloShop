using System.ComponentModel.DataAnnotations;
using MiloShop.Web.Validation;

namespace MiloShop.Web.Areas.Admin.ViewModels
{
    public class CategoryUpsertVm
    {
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [AllowedImage(MaxBytes = 3 * 1024 * 1024, AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" })]
        public IFormFile? ImageFile { get; set; }

        public string? ExistingImageUrl { get; set; }
    }
}
