using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using avatCo.Data;
using avatCo.Models;
using avatCo.Areas.Auth.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace avatCo.Services
{
    public class AuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly MiloDbContext _context;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            MiloDbContext context)
        {
            _userManager = userManager;
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
                    Errors = new Dictionary<string, string[]> {
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
                    Errors = new Dictionary<string, string[]> {
                        { "UserName", new[] { "نام کاربری قبلاً ثبت شده است" } }
                    }
                        };
                    }

            // 3. Create user
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                EmailConfirmed = false,
                TermsAcceptedAt = DateTime.UtcNow,
                TermsVersion = "v1.0"
            };


            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                // ✅ CORRECTLY MAP IDENTITY ERRORS TO FIELDS
                var identityErrors = createResult.Errors
                    .Select(e => new { Field = "Password", Message = e.Description })
                    .ToList();

                // ✅ BUILD PROPER ERROR DICTIONARY
                var errorsDict = new Dictionary<string, string[]>();
                foreach (var error in identityErrors)
                {
                    if (!errorsDict.ContainsKey(error.Field))
                        errorsDict[error.Field] = new string[] { };

                    errorsDict[error.Field] = errorsDict[error.Field]
                        .Concat(new[] { error.Message })
                        .ToArray();
                }

                return new Result
                {
                    Success = false,
                    Message = "خطا در ثبت نام",
                    Errors = errorsDict
                };
            }

            // ✅ FIXED LOGIC: ROLE ASSIGNMENT OUTSIDE ERROR BLOCK
            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }
            await _userManager.AddToRoleAsync(user, "User");

            // 4. Success
            return new Result
            {
                Success = true,
                User = user
            };

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