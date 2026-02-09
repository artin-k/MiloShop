using avatCo.Data;
using avatCo.Models;
using avatCo.Models.ViewModels;
using avatCo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace avatCo.Controllers
{
    [Authorize]
    [Route("Cart")]
    public class CartController : Controller
    {
        private readonly MiloDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICouponService _couponService;

        public CartController(MiloDbContext db, UserManager<ApplicationUser> userManager, ICouponService couponService)
        {
            _db = db;
            _userManager = userManager;
            _couponService = couponService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Redirect("/Auth/Login");

            var model = await BuildCartViewModelAsync(user.Id, null);
            return View(model);
        }

        [HttpPost("ApplyCoupon")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyCoupon(string? code)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Redirect("/Auth/Login");

            var model = await BuildCartViewModelAsync(user.Id, code);
            return View("Index", model);
        }

        [HttpGet("Add/{id}")]
        public async Task<IActionResult> Add(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Redirect("/Auth/Login");

            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            var item = await _db.CartItems
                .FirstOrDefaultAsync(c => c.UserId == user.Id && c.ProductId == id);

            if (item == null)
            {
                _db.CartItems.Add(new CartItem
                {
                    UserId = user.Id,
                    ProductId = id,
                    Quantity = 1
                });
            }
            else
            {
                item.Quantity += 1;
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Remove")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Redirect("/Auth/Login");

            var item = await _db.CartItems
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == user.Id);

            if (item == null) return NotFound();

            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Checkout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(string? couponCode)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Redirect("/Auth/Login");

            var cartItems = await _db.CartItems
                .Where(c => c.UserId == user.Id)
                .Include(c => c.Product)
                .ToListAsync();

            if (cartItems.Count == 0)
            {
                return RedirectToAction(nameof(Index));
            }

            var subtotal = cartItems.Sum(ci => (ci.Product?.Price ?? 0) * ci.Quantity);
            Coupon? coupon = null;
            decimal discount = 0;
            decimal finalTotal = subtotal;

            if (!string.IsNullOrWhiteSpace(couponCode))
            {
                var check = await _couponService.ValidateAndCalculateAsync(couponCode, subtotal);
                if (!check.Ok)
                {
                    var failedModel = await BuildCartViewModelAsync(user.Id, couponCode);
                    failedModel.CouponOk = false;
                    failedModel.CouponMessage = check.Message;
                    return View("Index", failedModel);
                }

                coupon = check.Coupon;
                discount = check.DiscountAmount;
                finalTotal = check.FinalAmount;
            }

            await using var tx = await _db.Database.BeginTransactionAsync();

            var order = new Order
            {
                UserId = user.Id,
                OrderedAt = DateTime.UtcNow,
                TotalAmount = subtotal,
                CouponId = coupon?.Id,
                CouponCode = coupon?.Code,
                DiscountAmount = discount,
                FinalAmount = finalTotal,
                Items = cartItems.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Product?.Price ?? 0
                }).ToList()
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            if (coupon != null)
            {
                var consume = await _couponService.ConsumeAsync(coupon.Id);
                if (!consume.ok)
                {
                    await tx.RollbackAsync();
                    var failedModel = await BuildCartViewModelAsync(user.Id, coupon.Code);
                    failedModel.CouponOk = false;
                    failedModel.CouponMessage = consume.message;
                    return View("Index", failedModel);
                }
            }

            _db.CartItems.RemoveRange(cartItems);
            await _db.SaveChangesAsync();

            await tx.CommitAsync();

            TempData["OrderPlaced"] = "Order placed successfully.";
            return Redirect("/Profile");
        }

        private async Task<CartViewModel> BuildCartViewModelAsync(string userId, string? code)
        {
            var cartItems = await _db.CartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToListAsync();

            var model = new CartViewModel
            {
                Items = cartItems.Select(ci => new CartItemViewModel
                {
                    Id = ci.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product?.Title ?? "(Deleted product)",
                    ProductImageUrl = ci.Product?.ImageUrl,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Product?.Price ?? 0
                }).ToList()
            };

            model.Subtotal = model.Items.Sum(i => i.UnitPrice * i.Quantity);
            model.FinalTotal = model.Subtotal;

            if (!string.IsNullOrWhiteSpace(code))
            {
                var result = await _couponService.ValidateAndCalculateAsync(code, model.Subtotal);
                model.CouponOk = result.Ok;
                model.CouponMessage = result.Message;
                model.CouponCode = result.Coupon?.Code ?? code.Trim().ToUpper();

                if (result.Ok)
                {
                    model.DiscountAmount = result.DiscountAmount;
                    model.FinalTotal = result.FinalAmount;
                }
            }

            return model;
        }
    }
}
