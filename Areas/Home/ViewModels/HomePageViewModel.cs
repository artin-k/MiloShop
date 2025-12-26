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


    }
}