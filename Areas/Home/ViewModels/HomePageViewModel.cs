using avatCo.Areas.Admin.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace avatCo.Models.ViewModels
{
    public class HomePageViewModel
    {
        public string HeroTitle { get; set; } = "Welcome to Avat Co";
        public string HeroSubtitle { get; set; } = "Scalable solutions for modern businesses";

/*        public List<ServiceItem> Services { get; set; } = new();
        public List<Product> Products { get; set; } = new();*/
  
        public List<Product> NewProducts { get; set; } = [];
        public List<Product> FeaturedProducts { get; set; } = [];
        public List<Category> Categories { get; set; } = [];
        public List<HeroSlideVm> HeroSlides { get; set; } = new();

        public Coupon? ActiveCoupon { get; set; }
        public Offer? ActiveOffer { get; set; }

        public string? SupportEmail { get; set; }
        public string? MainPhone { get; set; }
        public string? InstagramUrl { get; set; }
        public string? WhatsappUrl { get; set; }
        public string? TwitterUrl { get; set; }

    }
}