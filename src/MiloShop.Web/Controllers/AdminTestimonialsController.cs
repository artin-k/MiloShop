using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using MiloShop.Web.ViewModels.Admin;

namespace MiloShop.Web.Controllers;

[Authorize(Roles = "Admin")]
[AutoValidateAntiforgeryToken]
[Route("Admin/Testimonials")]
public class AdminTestimonialsController(MiloDbContext db) : Controller
{
    [HttpGet(Name = "AdminTestimonialsIndex")]
    public async Task<IActionResult> Index(int page = 1)
    {
        const int size = 25;
        int pages = Math.Max(1, (int)Math.Ceiling(await db.Reviews.CountAsync() / (double)size));
        page = Math.Clamp(page, 1, pages);
        ViewBag.Page = page;
        ViewBag.Pages = pages;
        return View(await db.Reviews.AsNoTracking().Include(r => r.Product)
            .OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id)
            .Skip((page - 1) * size).Take(size).ToListAsync());
    }

    [HttpGet("Create")]
    public Task<IActionResult> Create() => Editor(new TestimonialInput());

    [HttpPost("Create")]
    public async Task<IActionResult> Create(TestimonialInput input)
    {
        var product = await db.Products.FindAsync(input.ProductId);
        if (product == null) ModelState.AddModelError(nameof(input.ProductId), "محصول را انتخاب کنید.");
        if (!ModelState.IsValid) return await Editor(input);
        db.Reviews.Add(new Review
        {
            Product = product!, ProductId = input.ProductId, Comment = input.Quote.Trim(),
            UserName = input.Author.Trim(), Rating = input.Rating, HomepagePlacement = HomepagePlacement.Featured
        });
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> Editor(TestimonialInput input)
    {
        input.Products = await db.Products.AsNoTracking().OrderBy(p => p.Title)
            .Select(p => new SelectListItem(p.Title, p.Id.ToString())).ToListAsync();
        return View("Create", input);
    }

    [HttpPost("{id:int}")]
    public async Task<IActionResult> SetPlacement(int id, [Microsoft.AspNetCore.Mvc.ModelBinding.BindRequired] HomepagePlacement placement, int page)
    {
        if (!ModelState.IsValid || !Enum.IsDefined(placement)) return BadRequest();
        var review = await db.Reviews.FindAsync(id);
        if (review == null) return NotFound();
        review.HomepagePlacement = placement;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { page });
    }
}
