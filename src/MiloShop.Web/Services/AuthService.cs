using Microsoft.AspNetCore.Identity;
using MiloShop.Infrastructure.Persistence;
using MiloShop.Domain.Entities;
using MiloShop.Web.ViewModels.Auth;

namespace MiloShop.Web.Services
{
    public class AuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly MiloDbContext _context;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            MiloDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<Result> SignUpAsync(SignUpVM model)
        {
            // 1. Check email existence
            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                return new Result
                {
                    Success = false,
                    Message = "ایمیل قبلاً ثبت شده است",
                    Errors = new Dictionary<string, string[]>
                    {
                        { "Email", new[] { "ایمیل قبلاً ثبت شده است" } }
                    }
                };
            }

            // 2. Check username existence
            if (await _userManager.FindByNameAsync(model.UserName) != null)
            {
                return new Result
                {
                    Success = false,
                    Message = "نام کاربری قبلاً ثبت شده است",
                    Errors = new Dictionary<string, string[]>
                    {
                        { "UserName", new[] { "نام کاربری قبلاً ثبت شده است" } }
                    }
                };
            }

            // 3. Create user
            var user = new ApplicationUser
            {
                UserName = model.UserName.Trim(),
                Email = model.Email.Trim(),
                EmailConfirmed = false,
                TermsAcceptedAt = DateTime.UtcNow,
                TermsVersion = "v1.0"
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                // Map identity errors
                var errorsDict = new Dictionary<string, string[]>
                {
                    { "Password", createResult.Errors.Select(e => e.Description).ToArray() }
                };

                return new Result
                {
                    Success = false,
                    Message = "خطا در ثبت نام",
                    Errors = errorsDict
                };
            }

            // Role assignment
            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new IdentityRole("User"));

            await _userManager.AddToRoleAsync(user, "User");

            return new Result
            {
                Success = true,
                User = user
            };
        }

        //  NEW: LoginAsync using Identity + same Result type
        public async Task<Result> LoginAsync(LoginVM model)
        {
            var identifier = model.Identifier?.Trim();
            var password = model.Password;

            if (string.IsNullOrWhiteSpace(identifier))
            {
                return new Result
                {
                    Success = false,
                    Errors = new Dictionary<string, string[]>
            {
                { "Identifier", new[] { "ایمیل یا نام کاربری الزامی است." } }
            }
                };
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return new Result
                {
                    Success = false,
                    Errors = new Dictionary<string, string[]>
            {
                { "Password", new[] { "رمز عبور الزامی است." } }
            }
                };
            }

            // 1) try email first
            var user = await _userManager.FindByEmailAsync(identifier);

            // 2) if not found, try username
            user ??= await _userManager.FindByNameAsync(identifier);

            if (user == null)
            {
                return new Result
                {
                    Success = false,
                    Message = "ایمیل/نام کاربری یا رمز عبور اشتباه است.",
                    Errors = new Dictionary<string, string[]>
            {
                { "", new[] { "ایمیل/نام کاربری یا رمز عبور اشتباه است." } }
            }
                };
            }

            var signIn = await _signInManager.PasswordSignInAsync(
                user.UserName!, // ✅ همیشه با UserName ساین-این می‌کنیم
                password,
                isPersistent: model.RememberMe,
                lockoutOnFailure: true);

            if (!signIn.Succeeded)
            {
                return new Result
                {
                    Success = false,
                    Message = "ایمیل/نام کاربری یا رمز عبور اشتباه است.",
                    Errors = new Dictionary<string, string[]>
            {
                { "", new[] { "ایمیل/نام کاربری یا رمز عبور اشتباه است." } }
            }
                };
            }

            return new Result { Success = true, User = user };
        }

    }

    public class Result
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public ApplicationUser? User { get; set; }
        public Dictionary<string, string[]> Errors { get; set; } = new();
    }
}
