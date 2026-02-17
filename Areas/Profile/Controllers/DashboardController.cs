using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using avatCo.Data;
using avatCo.Models;
using avatCo.Areas.Profile.ViewModels;

namespace avatCo.Areas.Profile.Controllers
{
    [Area("Profile")]
    [Authorize]
    [Route("Profile")]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MiloDbContext _db;

        public DashboardController(UserManager<ApplicationUser> userManager, MiloDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        // GET /Profile
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Redirect("/Auth/Login");

            // Load cart items (and product info) for this user
            var cartItems = await _db.CartItems
                .Where(c => c.UserId == user.Id)
                .Include(c => c.Product)
                .ToListAsync();

            // Load orders count (or orders list if you want)
            var ordersCount = await _db.Orders.CountAsync(o => o.UserId == user.Id);

            var vm = new DashboardVM
            {
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                FullName = user.FullName,
                Gender = user.Gender,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                Address = user.Address,
                ProfileImageUrl = user.ProfileImageUrl,

                OrdersCount = ordersCount,
                CartItems = cartItems.Select(ci => new CartItemVM
                {
                    Id = ci.Id,
                    ProductName = ci.Product?.Title ?? "(Deleted product)",
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Product?.SalePrice ?? ci.Product?.Price ?? 0,
                    ProductImageUrl = ci.Product?.ImageUrl
                }).ToList()
            };

            return View(vm);
        }

        // GET /Profile/Edit
        [HttpGet("Edit")]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Redirect("/Auth/Login");

            var vm = new UpdateProfileVM
            {
                UserName = user.UserName ?? "",
                FullName = user.FullName,
                Gender = user.Gender,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                Address = user.Address
            };

            return View(vm);
        }

        // POST /Profile/Edit
        [HttpPost("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateProfileVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Redirect("/Auth/Login");

            var newUserName = vm.UserName.Trim();

            if (!string.Equals(user.UserName, newUserName, StringComparison.Ordinal))
            {
                var setName = await _userManager.SetUserNameAsync(user, newUserName);
                if (!setName.Succeeded)
                {
                    foreach (var e in setName.Errors)
                        ModelState.AddModelError("", e.Description);
                    return View(vm);
                }
            }

            user.FullName = vm.FullName?.Trim();
            user.Address = vm.Address?.Trim();
            user.PhoneNumber = vm.PhoneNumber?.Trim();
            user.Gender = string.IsNullOrWhiteSpace(vm.Gender) ? null : vm.Gender;
            user.DateOfBirth = vm.DateOfBirth;

            var update = await _userManager.UpdateAsync(user);
            if (!update.Succeeded)
            {
                foreach (var e in update.Errors)
                    ModelState.AddModelError("", e.Description);
                return View(vm);
            }

            TempData["ProfileSaved"] = "پروفایل با موفقیت به‌روزرسانی شد.";
            return RedirectToAction(nameof(Index));
        }
    }
}
