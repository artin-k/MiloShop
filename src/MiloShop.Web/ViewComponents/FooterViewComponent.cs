using Microsoft.AspNetCore.Mvc;

namespace MiloShop.Web.ViewComponents;

public class FooterViewComponent(ISiteSettingsService settings) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync() =>
        View("~/Views/Shared/_Footer.cshtml", await settings.GetManyAsync(
            "MainPhone", "SupportEmail", "OfficeAddress", "InstagramUrl", "WhatsappUrl", "TwitterUrl",
            "Home.DeskVisibility", "Home.FeaturesVisibility"));
}
