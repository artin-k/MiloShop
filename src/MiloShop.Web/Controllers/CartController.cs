using MiloShop.Infrastructure.Persistence;
using MiloShop.Domain.Entities;
using MiloShop.Web.ViewModels;
using MiloShop.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MiloShop.Web.Controllers
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

        [HttpPost("Add/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int id, string? returnUrl = null, int quantity = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Redirect("/Auth/Login");

            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();
            if (!product.IsActive || product.StockQuantity <= 0)
            {
                TempData["OrderError"] = "این محصول در حال حاضر موجود نیست.";
                return RedirectToAction(nameof(Index));
            }

            var item = await _db.CartItems
                .FirstOrDefaultAsync(c => c.UserId == user.Id && c.ProductId == id);

            if (!ModelState.IsValid || quantity < 1 || quantity > product.StockQuantity - (item?.Quantity ?? 0))
            {
                TempData["OrderError"] = "تعداد درخواستی با موجودی محصول سازگار نیست.";
                return RedirectToAction(nameof(Index));
            }

            if (item == null)
            {
                _db.CartItems.Add(new CartItem
                {
                    UserId = user.Id,
                    ProductId = id,
                    Quantity = quantity
                });
            }
            else
            {
                item.Quantity += quantity;
            }

            await _db.SaveChangesAsync();

            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl!);
            }

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

        [HttpGet("Checkout")]
        public async Task<IActionResult> Checkout(string? couponCode)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Redirect("/Auth/Login");

            var model = await BuildCheckoutViewModelAsync(user, couponCode);
            if (model.Items.Count == 0)
            {
                TempData["OrderError"] = "سبد خرید شما خالی است.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpPost("Checkout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel input)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Redirect("/Auth/Login");

            var model = await BuildCheckoutViewModelAsync(user, input.CouponCode);
            ApplyCheckoutInput(model, input);

            if (model.Items.Count == 0)
            {
                TempData["OrderError"] = "سبد خرید شما خالی است.";
                return RedirectToAction(nameof(Index));
            }

            if (!string.IsNullOrWhiteSpace(model.CouponCode) && !model.CouponOk)
            {
                ModelState.AddModelError(nameof(CheckoutViewModel.CouponCode), model.CouponMessage ?? "کد تخفیف نامعتبر است.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var cartItems = await _db.CartItems
                .Where(c => c.UserId == user.Id)
                .Include(c => c.Product)
                .ToListAsync();

            var subtotal = cartItems.Sum(ci => GetEffectivePrice(ci.Product) * ci.Quantity);
            Coupon? coupon = null;
            decimal discount = 0;
            decimal finalTotal = subtotal;

            if (!string.IsNullOrWhiteSpace(model.CouponCode))
            {
                var check = await _couponService.ValidateAndCalculateAsync(model.CouponCode, subtotal);
                if (!check.Ok)
                {
                    model.CouponOk = false;
                    model.CouponMessage = check.Message;
                    ModelState.AddModelError(nameof(CheckoutViewModel.CouponCode), check.Message ?? "کد تخفیف نامعتبر است.");
                    return View(model);
                }

                coupon = check.Coupon;
                discount = check.DiscountAmount;
                finalTotal = check.FinalAmount;
            }

            user.FullName = model.FullName.Trim();
            user.PhoneNumber = model.PhoneNumber.Trim();
            user.Address = model.Address.Trim();
            var userUpdate = await _userManager.UpdateAsync(user);
            if (!userUpdate.Succeeded)
            {
                foreach (var error in userUpdate.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            await using var tx = await _db.Database.BeginTransactionAsync();

            foreach (var cartItem in cartItems)
            {
                var affectedRows = await _db.Products
                    .Where(product => product.Id == cartItem.ProductId)
                    .Where(product => product.IsActive)
                    .Where(product => product.StockQuantity >= cartItem.Quantity)
                    .ExecuteUpdateAsync(
                        setters => setters.SetProperty(
                            product => product.StockQuantity,
                            product => product.StockQuantity - cartItem.Quantity),
                        HttpContext.RequestAborted);

                if (affectedRows == 1)
                {
                    continue;
                }

                await tx.RollbackAsync();
                ModelState.AddModelError(
                    string.Empty,
                    $"موجودی «{cartItem.Product?.Title ?? "محصول"}» برای این سفارش کافی نیست.");
                return View(model);
            }

            var order = new Order
            {
                UserId = user.Id,
                OrderedAt = DateTime.UtcNow,
                TotalAmount = subtotal,
                CouponId = coupon?.Id,
                CouponCode = coupon?.Code,
                DiscountAmount = discount,
                FinalAmount = finalTotal,
                RecipientFullName = model.FullName.Trim(),
                RecipientPhoneNumber = model.PhoneNumber.Trim(),
                ShippingAddress = model.Address.Trim(),
                PostalCode = string.IsNullOrWhiteSpace(model.PostalCode) ? null : model.PostalCode.Trim(),
                CustomerNote = string.IsNullOrWhiteSpace(model.Note) ? null : model.Note.Trim(),
                Status = Order.PendingReviewStatus,
                Items = cartItems.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    UnitPrice = GetEffectivePrice(ci.Product)
                }).ToList()
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            if (coupon != null)
            {
                var consume = await _couponService.ConsumeAsync(coupon.Id);
                if (!consume.Succeeded)
                {
                    await tx.RollbackAsync();
                    model.CouponOk = false;
                    model.CouponMessage = consume.Message;
                    ModelState.AddModelError(nameof(CheckoutViewModel.CouponCode), consume.Message ?? "خطا در اعمال کد تخفیف.");
                    return View(model);
                }
            }

            _db.CartItems.RemoveRange(cartItems);
            await _db.SaveChangesAsync();

            await tx.CommitAsync();

            TempData["OrderPlaced"] = "سفارش شما با موفقیت ثبت شد.";
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
                    ProductName = ci.Product?.Title ?? "(محصول حذف شده)",
                    ProductImageUrl = ci.Product?.ImageUrl,
                    Quantity = ci.Quantity,
                    UnitPrice = GetEffectivePrice(ci.Product)
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

        private async Task<CheckoutViewModel> BuildCheckoutViewModelAsync(ApplicationUser user, string? code)
        {
            var cartItems = await _db.CartItems
                .Where(c => c.UserId == user.Id)
                .Include(c => c.Product)
                .ToListAsync();

            var model = new CheckoutViewModel
            {
                Items = cartItems.Select(ci => new CartItemViewModel
                {
                    Id = ci.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product?.Title ?? "(محصول حذف شده)",
                    ProductImageUrl = ci.Product?.ImageUrl,
                    Quantity = ci.Quantity,
                    UnitPrice = GetEffectivePrice(ci.Product)
                }).ToList(),
                FullName = user.FullName ?? user.UserName ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Address = user.Address ?? string.Empty,
                CouponCode = string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpper()
            };

            model.Subtotal = model.Items.Sum(i => i.UnitPrice * i.Quantity);
            model.FinalTotal = model.Subtotal;

            if (!string.IsNullOrWhiteSpace(model.CouponCode))
            {
                var result = await _couponService.ValidateAndCalculateAsync(model.CouponCode, model.Subtotal);
                model.CouponOk = result.Ok;
                model.CouponMessage = result.Message;
                model.CouponCode = result.Coupon?.Code ?? model.CouponCode;

                if (result.Ok)
                {
                    model.DiscountAmount = result.DiscountAmount;
                    model.FinalTotal = result.FinalAmount;
                }
            }

            return model;
        }

        private static void ApplyCheckoutInput(CheckoutViewModel model, CheckoutViewModel input)
        {
            model.FullName = input.FullName?.Trim() ?? string.Empty;
            model.PhoneNumber = input.PhoneNumber?.Trim() ?? string.Empty;
            model.Address = input.Address?.Trim() ?? string.Empty;
            model.PostalCode = input.PostalCode?.Trim();
            model.Note = input.Note?.Trim();
            model.ConfirmPurchase = input.ConfirmPurchase;
            model.CouponCode = input.CouponCode?.Trim().ToUpper();
        }

        private static decimal GetEffectivePrice(Product? product)
        {
            if (product == null) return 0;
            return product.SalePrice ?? product.Price;
        }
    }
}
