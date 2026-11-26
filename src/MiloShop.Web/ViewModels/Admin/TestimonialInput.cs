using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MiloShop.Web.ViewModels.Admin;

public class TestimonialInput
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }
    [Required, StringLength(100)]
    public string Author { get; set; } = "";
    [Required, StringLength(1000)]
    public string Quote { get; set; } = "";
    [Range(1, 5)]
    public int Rating { get; set; } = 5;
    [BindNever]
    public List<SelectListItem> Products { get; set; } = [];
}
