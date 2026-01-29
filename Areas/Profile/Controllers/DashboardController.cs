using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using avatCo.Models;
using avatCo.Areas.Profile.ViewModels;

namespace avatCo.Areas.Profile.Controllers
{
    [Area("Profile")]
    [Authorize]
    [Route("Profile")] // ✅ makes this controller respond under /Profile
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET /Profile
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Redirect("/Auth/Login");

            var vm = new DashboardVM
            {
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                FullName = user.FullName,
                Gender = user.Gender,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                Address = user.Address
            };

            return View(vm);
        }

        // POST /Profile/UpdateProfile
        [HttpPost("UpdateProfile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileVM vm)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelStateToText(ModelState));

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized("You are not logged in.");

            // ✅ Basic normalization
            vm.UserName = vm.UserName.Trim();

            // ✅ Optional: validate Gender values
            if (!string.IsNullOrWhiteSpace(vm.Gender))
            {
                var allowed = new[] { "Male", "Female", "Other" };
                if (!allowed.Contains(vm.Gender))
                    return BadRequest("Invalid gender value.");
            }

            // ✅ If username changed, use Identity API (important)
            if (!string.Equals(user.UserName, vm.UserName, StringComparison.Ordinal))
            {
                var setUserNameResult = await _userManager.SetUserNameAsync(user, vm.UserName);
                if (!setUserNameResult.Succeeded)
                    return BadRequest(IdentityErrorsToText(setUserNameResult.Errors));
            }

            // ✅ Update custom fields
            user.FullName = vm.FullName?.Trim();
            user.PhoneNumber = vm.PhoneNumber?.Trim();
            user.Gender = string.IsNullOrWhiteSpace(vm.Gender) ? null : vm.Gender;
            user.DateOfBirth = vm.DateOfBirth;
            user.Address = vm.Address?.Trim();

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return BadRequest(IdentityErrorsToText(updateResult.Errors));

            // ✅ Return JSON for fetch()
            return Ok(new
            {
                success = true,
                message = "Saved successfully",
                updated = new
                {
                    userName = user.UserName,
                    fullName = user.FullName,
                    phoneNumber = user.PhoneNumber,
                    gender = user.Gender,
                    dateOfBirth = user.DateOfBirth?.ToString("yyyy-MM-dd"),
                    address = user.Address
                }
            });
        }

        private static string IdentityErrorsToText(IEnumerable<IdentityError> errors)
            => string.Join(" | ", errors.Select(e => e.Description));

        private static string ModelStateToText(Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState)
        {
            var all = modelState
                .Where(kvp => kvp.Value?.Errors.Count > 0)
                .SelectMany(kvp => kvp.Value!.Errors.Select(e => $"{kvp.Key}: {e.ErrorMessage}"));

            return string.Join(" | ", all);
        }
    }
}