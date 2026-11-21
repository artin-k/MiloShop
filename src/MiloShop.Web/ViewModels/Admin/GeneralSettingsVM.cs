using System.ComponentModel.DataAnnotations;

namespace MiloShop.Web.ViewModels.Admin
{
    public class GeneralSettingsVM
    {
        [Display(Name = "شماره تماس اصلی")]
        public string? MainPhone { get; set; }

        [Display(Name = "ایمیل پشتیبانی")]
        public string? SupportEmail { get; set; }

        [Display(Name = "آدرس دفتر مرکزی")]
        public string? OfficeAddress { get; set; }

        public string? WhatsappUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? TwitterUrl { get; set; }
    }
}
