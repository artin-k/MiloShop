using MiloShop.Domain.Entities;

namespace MiloShop.Web.ViewModels.Home
{
    public class HomePageViewModel
    {
        public List<Product> NewProducts { get; set; } = [];
        public List<Category> Categories { get; set; } = [];
        public Coupon? ActiveCoupon { get; set; }
        public Offer? ActiveOffer { get; set; }

    }
}
