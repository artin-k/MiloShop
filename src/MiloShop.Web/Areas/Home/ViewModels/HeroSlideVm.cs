namespace MiloShop.Web.ViewModels
{
    public class HeroSlideVm
    {
        public string Title { get; set; } = "";
        public string Subtitle { get; set; } = "";
        public string BackgroundImageUrl { get; set; } = ""; // "/uploads/hero/xxx.jpg" or "/images/hero/1.jpg"
        public string Badge { get; set; } = "";              // e.g. "۴ قسمت"
        public string CtaText { get; set; } = "مشاهده";
        public string CtaUrl { get; set; } = "#";
        public string? ThumbImageUrl { get; set; }           // optional thumbnail
    }
}
