using System.Collections.Generic;
using avatCo.Models;

namespace avatCo.Areas.Shop.ViewModels
{
    public class ShopViewModel
    {
        public string ShopName { get; set; } = string.Empty;
        public string ShopDescription { get; set; } = string.Empty;
        public List<Product> Products { get; set; } = new();// ✅ correct type
        public List<Category> Categories { get; set; } = new(); // ✅ correct type
        public List<Review> Reviews { get; set; } = new(); // ✅ correct type
        public string? Query { get; set; }
        public string? Sort { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? CategoryId { get; set; }


        //
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string Brand { get; set; } = "";
        public string Material { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public bool IsActive { get; set; }
        public decimal Price { get; set; }




    }
}
