using avatCo.Models;
using avatCo.Areas.Profile.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using avatCo.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;


namespace avatCo.Areas.Profile.Controllers
{
    [Area("Profile")]
    [Route("Profile")]
    public class DashboardController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly MiloDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;


        public DashboardController(MiloDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _env = env;
        }

        [HttpGet("")]
        public async Task<IActionResult> Profile()
        {
            // Get user ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("Login", "Dashboard");
            }

            // Load user with cart items (optional)
            var user = await _context.Users
                .Include(u => u.Orders)
                .Include(u => u.CartItems)
                .ThenInclude(c => c.Product)
                .FirstOrDefaultAsync(u => u.Id == userId);


            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpGet("GetProfileData")]
        public async Task<IActionResult> GetProfileData()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || !int.TryParse(userId, out int id)) return Unauthorized();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var model = new EditProfileViewModel
            {
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber,
                ProfileImageUrl = user.ProfileImageUrl,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth
            };

            return Json(model);
        }

        [HttpPost("UpdateProfile")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile([FromForm] EditProfileViewModel model, IFormFile? ProfileImageFile)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid data" });

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            await using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                user.FullName = model.FullName;
                user.PhoneNumber = model.PhoneNumber;
                user.Address = model.Address;
                user.Gender = model.Gender;
                user.DateOfBirth = model.DateOfBirth;

                if (ProfileImageFile is { Length: > 0 })
                {
                    const long maxBytes = 2 * 1024 * 1024; // 2MB
                    if (ProfileImageFile.Length > maxBytes)
                        return BadRequest(new { success = false, message = "File too large" });

                    var ext = Path.GetExtension(ProfileImageFile.FileName).ToLowerInvariant();
                    var allowed = new HashSet<string> { ".jpg", ".jpeg", ".png", ".webp" };
                    if (!allowed.Contains(ext))
                        return BadRequest(new { success = false, message = "Invalid file type" });

                    var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "Profiles");
                    Directory.CreateDirectory(uploadsRoot);

                    var safeName = $"{Guid.NewGuid():N}{ext}";
                    var fullPath = Path.Combine(uploadsRoot, safeName);

                    await using var fs = new FileStream(fullPath, FileMode.CreateNew);
                    await ProfileImageFile.CopyToAsync(fs);

                    user.ProfileImageUrl = $"/uploads/Profiles/{safeName}";
                }

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                    return BadRequest(new { success = false, message = "Update failed" });

                await tx.CommitAsync();
                return Ok(new { success = true });
            }
            catch
            {
                await tx.RollbackAsync();
                return StatusCode(500, new { success = false, message = "Server error" });
            }
        }



        [HttpGet("Register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost("Register")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            await using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = new ApplicationUser
                {
                    UserName = vm.Email.Trim(),
                    Email = vm.Email.Trim()
                };

                var result = await _userManager.CreateAsync(user, vm.Password);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "ثبت نام ناموفق بود.");
                    return View(vm);
                }

                // Example: your own extra tables (optional)
                // _context.UserProfiles.Add(new UserProfile { UserId = user.Id, ... });
                // await _context.SaveChangesAsync();

                await tx.CommitAsync();

                // Safer mode: require email confirm, don't sign in here
                await _signInManager.SignInAsync(user, isPersistent: true);
                return Redirect("/");
            }
            catch
            {
                await tx.RollbackAsync();
                ModelState.AddModelError("", "ثبت نام ناموفق بود.");
                return View(vm);
            }
        }


        [HttpGet("Login")]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM vm, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(vm);

            returnUrl ??= "/";

            // IMPORTANT: only allow local redirects (prevents open redirect attacks)
            if (!Url.IsLocalUrl(returnUrl)) returnUrl = "/";

            var result = await _signInManager.PasswordSignInAsync(
                vm.Email.Trim(),
                vm.Password,
                vm.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
                return LocalRedirect(returnUrl);

            // Keep message generic
            ModelState.AddModelError("", "ورود ناموفق بود.");
            return View(vm);
        }



        [HttpPost("Logout")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }



    }
}
