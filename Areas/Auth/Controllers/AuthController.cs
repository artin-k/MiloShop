using Microsoft.AspNetCore.Mvc;
using avatCo.Models;
using avatCo.Areas.Auth.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using avatCo.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using avatCo.Services;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.IdentityModel.Tokens;


namespace avatCo.Areas.Auth.Controllers
{
    [Route("Auth")]
    [Area("Auth")]
    public class AuthController : Controller
    {
        private readonly MiloDbContext _context;
        private readonly AuthService _authService;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthController(
            MiloDbContext context,
            AuthService authController,
            SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _authService = authController;
            _signInManager = signInManager;
        }

    [HttpGet("Login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet("SignUp")]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost("SignUp")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(SignUpVM model)
        {
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

            // ✅ Sign in
            await _signInManager.SignInAsync(user, isPersistent: false);

            // ✅ Leave the Auth area and go to /Profile
            return RedirectToAction("Index", "Dashboard", new { area = "Profile" });

            // or the most bulletproof:
            // return Redirect("/Profile");
        }

        [HttpPost("Logout")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Auth", new { area = "Auth" });
        }

    }
}
