using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using avatCo.Data;
using avatCo.Services;
using avatCo.Models;

var builder = WebApplication.CreateBuilder(args);

// Register your DbContext as before
builder.Services.AddDbContext<MiloDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;

    // password policy
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;

    // brute force protection
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
})
.AddEntityFrameworkStores<MiloDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Profile/Login";
    opt.LogoutPath = "/Profile/Logout";
    opt.AccessDeniedPath = "/Profile/Login";

    opt.Cookie.HttpOnly = true;
    opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    opt.Cookie.SameSite = SameSiteMode.Lax; // Strict can break external login flows

    opt.SlidingExpiration = true;
    opt.ExpireTimeSpan = TimeSpan.FromDays(7);
});

builder.Services.AddScoped<IProductImageStorage, ProductImageStorage>();


// Add MVC (controllers + views)
builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseHsts();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseExceptionHandler("/error");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Enable attribute-based routing
app.MapControllers();

// Conventional area route (fallback for controller/action/id patterns)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();