using System.ComponentModel.DataAnnotations;
using MiloShop.Web.Validation;

namespace MiloShop.Web.ViewModels.Admin
{
    public class CategoryUpsertVm
    {
        [RegularExpression("^(notebook|pen|desk|planner|art|gift)$")]
        public string? Icon { get; set; }
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [AllowedImage(AllowSvg = true)]
        public IFormFile? ImageFile { get; set; }

        public string? ExistingImageUrl { get; set; }
    }
}
