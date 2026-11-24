using Microsoft.AspNetCore.Mvc;
using MiloShop.Web.ViewModels.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using MiloShop.Web.Services;


namespace MiloShop.Web.Controllers
{
    [Route("Auth")]

    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MiloDbContext _context;
        private readonly AuthService _authService;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthController(
            MiloDbContext context,
            AuthService authController,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _authService = authController;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet("Login")]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl!);

                return Redirect("/Profile");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpGet("SignUp")]
        public IActionResult SignUp(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpGet("AccessDenied")]
        public IActionResult AccessDenied()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View();
        }

        [HttpPost("SignUp")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(SignUpVM model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var result = await _authService.SignUpAsync(model);

            if (!result.Success)
            {
                foreach (var fieldErrors in result.Errors)
                {
                    foreach (var error in fieldErrors.Value)
                    {
                        Console.WriteLine($"Error for {fieldErrors.Key}: {error}");
                        ModelState.AddModelError(fieldErrors.Key, error);
                    }
                }
                return View(model);
            }

            var user = result.User!;

            // Sign in
            await _signInManager.SignInAsync(user, isPersistent: false);

            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl!);

            //  Leave the Auth area and go to /Profile
            return RedirectToAction("Index", "Profile", new { area = "" });

            // or the most bulletproof:
            // return Redirect("/Profile");
        }

        [HttpPost("Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM loginVM, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid) return View(loginVM);

            var result = await _authService.LoginAsync(loginVM);

            if (!result.Success)
            {
                foreach (var fieldErrors in result.Errors)
                {
                    foreach (var error in fieldErrors.Value)
                    {
                        Console.WriteLine($"Error for {fieldErrors.Key}: {error}");
                        ModelState.AddModelError(fieldErrors.Key, error);
                    }
                }                    
                return View(loginVM);
            }

            var user = result.User!;

            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl!);
            }

            // if admin → send to admin dashboard
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return RedirectToRoute("AdminDashboard");
            }

            return Redirect("/Profile");
        }


        [HttpPost("Logout")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Auth", new { area = "" });
        }

    }
}
