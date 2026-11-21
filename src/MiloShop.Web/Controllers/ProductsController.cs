using Microsoft.AspNetCore.Mvc;
using MiloShop.Application.Catalog;

namespace MiloShop.Web.Controllers;

public class ProductsController(ICatalogQueries catalog) : Controller
{
    [HttpGet("products/{id:int}/{slug?}", Name = "ProductDetails")]
    public async Task<IActionResult> Details(int id, string? slug = null)
    {
        var product = await catalog.GetProductAsync(id);
        if (product is null) return NotFound();
        if (!string.IsNullOrWhiteSpace(product.Slug) && slug != product.Slug)
            return RedirectToRoutePermanent("ProductDetails", new { id, slug = product.Slug });
        return View(product);
    }
}
