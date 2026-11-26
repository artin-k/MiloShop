using MiloShop.Domain.Entities;

namespace MiloShop.Web.ViewModels.Home
{
    public class HomePageViewModel
    {
        public Dictionary<string, string> Content { get; set; } = [];
        public List<Product> NewProducts { get; set; } = [];
        public List<Category> Categories { get; set; } = [];
        public List<MiloShop.Application.Catalog.CategoryCollection> Collections { get; set; } = [];
        public List<MiloShop.Application.Catalog.ProductTestimonial> Testimonials { get; set; } = [];
        public Coupon? ActiveCoupon { get; set; }
        public Offer? ActiveOffer { get; set; }

    }
}
