using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiloShop.Web.Models;
using MiloShop.Web.ViewModels.Home;

namespace MiloShop.Web.Controllers;

[Authorize(Roles = "Admin")]
[AutoValidateAntiforgeryToken]
[Route("Admin/Homepage")]
public class AdminHomepageController(ISiteSettingsService settings) : Controller
{
    [HttpGet(Name = "AdminHomepageIndex")]
    public async Task<IActionResult> Index() => View(await HomepageContent.LoadAsync(settings));

    [HttpPost]
    public async Task<IActionResult> Save(string group, Dictionary<string, string> values)
    {
        var fields = HomepageContent.Fields.Where(f => f.Group == group).ToList();
        if (fields.Count == 0) return BadRequest();
        var changes = new Dictionary<string, string?>();
        foreach (var field in fields)
        {
            var value = values.GetValueOrDefault(field.Key)?.Trim();
            if (string.IsNullOrEmpty(value) || value.Length > 500 ||
                (field.IsVisibility && value is not ("visible" or "hidden")) ||
                (field.IsIcon && !StationeryIcon.Options.ContainsKey(value)))
            {
                ModelState.AddModelError("", $"مقدار «{field.Label}» معتبر نیست (۱ تا ۵۰۰ نویسه).");
            }
            changes["Home." + field.Key] = value;
        }
        if (!ModelState.IsValid)
        {
            var content = await HomepageContent.LoadAsync(settings);
            foreach (var field in fields) content[field.Key] = values.GetValueOrDefault(field.Key) ?? "";
            return View("Index", content);
        }
        await settings.SetManyAsync(changes);
        TempData["Saved"] = group;
        return RedirectToAction(nameof(Index));
    }
}
