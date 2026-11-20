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
    options.Password.RequiredLength = 1;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredUniqueChars = 0;

    // brute force protection
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
})
.AddEntityFrameworkStores<MiloDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Auth/Login";
    opt.LogoutPath = "/Auth/Logout";        // if you have it
    opt.AccessDeniedPath = "/Auth/Login";   // or "/Auth/AccessDenied" if you make one

    opt.Cookie.IsEssential = true;
    opt.Cookie.HttpOnly = true;
    opt.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // dev
    opt.Cookie.SameSite = SameSiteMode.Lax;

    opt.SlidingExpiration = true;
    // Used for persistent cookies when RememberMe is enabled.
    opt.ExpireTimeSpan = TimeSpan.FromDays(30);
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = _ => true;
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
    options.Secure = CookieSecurePolicy.SameAsRequest;
});

builder.Services.Configure<Microsoft.AspNetCore.Mvc.CookieTempDataProviderOptions>(options =>
{
    options.Cookie.IsEssential = true;
});

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddScoped<IProductImageStorage, ProductImageStorage>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<ISiteSettingsService, SiteSettingsService>();


// Add MVC (controllers + views)
builder.Services.AddControllersWithViews();

var app = builder.Build();

await avatCo.Data.IdentitySeeder.SeedRolesAndAdminAsync(app.Services, app.Configuration);

app.UseHsts();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseExceptionHandler("/error");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapAreaControllerRoute(
    name: "AuthArea",
    areaName: "Auth",
    pattern: "Auth/{action=Login}/{id?}" // ✅ THIS IS THE FIX
);

//  THEN REGISTER REGULAR ROUTES
app.MapControllers();
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "Profile",
    pattern: "{area:exists}/{controller=Profile}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
