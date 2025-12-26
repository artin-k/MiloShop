using avatCo.Models;
using avatCo.Models.ViewModel;
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


namespace avatCo.Areas.Profile.Controllers
{
    [Area("Profile")]
    [Route("Profile")]
    public class DashboardController : Controller
    {
        private readonly MiloDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;


        public DashboardController(MiloDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
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
        public async Task<IActionResult> UpdateProfile([FromForm] EditProfileViewModel model, IFormFile? ProfileImageFile)
        {
            Console.WriteLine("Edit profile hit");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null) return NotFound();

            user.UserName = model.UserName;
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.Gender = model.Gender;
            user.DateOfBirth = model.DateOfBirth;

            if (ProfileImageFile != null && ProfileImageFile.Length > 0)
            {
                var filePath = Path.Combine("wwwroot/uploads/Profiles", ProfileImageFile.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfileImageFile.CopyToAsync(stream);
                }
                user.ProfileImageUrl = "/uploads/Profiles/" + ProfileImageFile.FileName;
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }


        [HttpGet("Register")]
        public IActionResult Register()
        {
            return View();
        }

        /*        [HttpPost("Register")]
                public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
                {
                    if (string.IsNullOrWhiteSpace(model.Password))
                        return BadRequest(new { success = false, message = "Password required" });

                    // Hash before saving
                    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password.Trim());

                    var user = new User
                    {
                        UserName = model.Username,   // 👈 fixed naming
                        Email = model.Email,
                        PasswordHash = hashedPassword
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    return Ok(new { success = true, redirectUrl = "/Profile" });
                }*/

        [HttpPost("Register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var identityUser = new ApplicationUser
            {
                UserName = vm.Email,
                Email = vm.Email
            };

            var result = await _userManager.CreateAsync(identityUser, vm.Password);

            if (!result.Succeeded)
            {
                // generic message to avoid enumeration
                ModelState.AddModelError("", "ثبت نام ناموفق بود.");
                return View(vm);
            }

            try
            {
                // OPTIONAL: if you want to create extra rows in your own tables,
                // do it here (Profile table etc.)

                // If you want max security: DON'T auto sign-in until email confirmed.
                await _signInManager.SignInAsync(identityUser, isPersistent: true);

                return Redirect("/");
            }
            catch
            {
                await _userManager.DeleteAsync(identityUser);
                ModelState.AddModelError("", "ثبت نام ناموفق بود.");
                return View(vm);
            }
        }




        [HttpGet("Login")]
        public IActionResult Login()
        {
            return View();
        }

        /*        [HttpPost("Login")]
                public async Task<IActionResult> Login([FromBody] LoginViewModel model)
                {
                    if (!ModelState.IsValid)
                    {
                        Console.WriteLine("not valid");
                        //return BadRequest(new { success = false, message = "Invalid model state" });
                    }

                    string hash = "$2a$11$EXU1DdhCIHVWWUKQpG18KeIhtfCzeiPKRbC2xepkSgo/VyXi2Y.AO";
                    bool ok = BCrypt.Net.BCrypt.Verify("1", hash);
                    Console.WriteLine($"Manual verify: {ok}");


                    if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
                        return BadRequest(new { success = false, message = "Email and Password are required." });

                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                    if (user == null)
                        return Unauthorized(new { success = false, message = "Invalid Email." });

                    Console.WriteLine($"[DEBUG] Password: {model.Password}, Hash: {user.PasswordHash}");

                    if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                        return Unauthorized(new { success = false, message = "Invalid Password." });


                    // 🔹 Create claims
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.UserName ?? user.Email),
                        new Claim(ClaimTypes.Email, user.Email)
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    // 🔹 Sign in user with cookie
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        principal,
                        new AuthenticationProperties { IsPersistent = true });

                    // 🔹 Return JSON response for your JS fetch
                    return Ok(new { success = true, redirectUrl = "/Profile" });
                }*/

        [HttpPost("Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM vm, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _signInManager.PasswordSignInAsync(
                vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
                return LocalRedirect(returnUrl ?? "/");

            // Generic message (don’t reveal locked out / user exists / etc.)
            ModelState.AddModelError("", "ورود ناموفق بود.");
            return View(vm);
        }


        [HttpPost("Logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
 //           await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Profile", new { area = "UserProfile" });
        }


    }
}
