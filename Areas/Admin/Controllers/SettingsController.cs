using avatCo.Areas.Admin.ViewModels;
using avatCo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace avatCo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [AutoValidateAntiforgeryToken]
    public class SettingsController : Controller
    {
        private readonly ISiteSettingsService _settings;
        public SettingsController(ISiteSettingsService settings) => _settings = settings;

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
