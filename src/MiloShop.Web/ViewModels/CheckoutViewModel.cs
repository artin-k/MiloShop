using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MiloShop.Web.ViewModels
{
    public class CheckoutViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();

        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalTotal { get; set; }

        public bool CouponOk { get; set; }
        public string? CouponMessage { get; set; }
        public string? CouponCode { get; set; }

        [Required(ErrorMessage = "نام و نام خانوادگی را وارد کنید.")]
        [MaxLength(120)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "شماره تماس را وارد کنید.")]
        [MaxLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "آدرس تحویل سفارش را وارد کنید.")]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        [MaxLength(800)]
        public string? Note { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "برای ثبت سفارش باید تایید نهایی را فعال کنید.")]
        public bool ConfirmPurchase { get; set; }
    }
}
