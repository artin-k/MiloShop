using MiloShop.Web.ViewModels.Admin;
using MiloShop.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MiloShop.Web.Controllers
{
    [Route("Admin/Settings/[action]/{id?}")]
    [Authorize(Roles = "Admin")]
    [AutoValidateAntiforgeryToken]
    public class AdminSettingsController : Controller
    {
        private readonly ISiteSettingsService _settings;
        public AdminSettingsController(ISiteSettingsService settings) => _settings = settings;

        [HttpGet]
        [HttpGet("/Admin/Settings", Name = "AdminSettingsIndex")]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(General));
        }

        [HttpGet]
        public async Task<IActionResult> General()
        {
            var dict = await _settings.GetManyAsync(
                "MainPhone", "SupportEmail", "OfficeAddress",
                "WhatsappUrl", "InstagramUrl", "TwitterUrl"
            );

            var vm = new GeneralSettingsVM
            {
                MainPhone = dict["MainPhone"],
                SupportEmail = dict["SupportEmail"],
                OfficeAddress = dict["OfficeAddress"],
                WhatsappUrl = dict["WhatsappUrl"],
                InstagramUrl = dict["InstagramUrl"],
                TwitterUrl = dict["TwitterUrl"],
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> General(GeneralSettingsVM vm)
        {
            await _settings.SetManyAsync(new Dictionary<string, string?>
            {
                ["MainPhone"] = vm.MainPhone,
                ["SupportEmail"] = vm.SupportEmail,
                ["OfficeAddress"] = vm.OfficeAddress,
                ["WhatsappUrl"] = vm.WhatsappUrl,
                ["InstagramUrl"] = vm.InstagramUrl,
                ["TwitterUrl"] = vm.TwitterUrl,
            });

            TempData["ok"] = "ذخیره شد ✅";
            return RedirectToAction(nameof(General));
        }
    }
}
