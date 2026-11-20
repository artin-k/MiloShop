using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using MiloShop.Web.Services;
using MiloShop.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;

    // password policy
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredUniqueChars = 1;

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

    opt.Cookie.HttpOnly = true;
    opt.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // dev
    opt.Cookie.SameSite = SameSiteMode.Lax;

    opt.SlidingExpiration = true;
    // Used for persistent cookies when RememberMe is enabled.
    opt.ExpireTimeSpan = TimeSpan.FromDays(30);
});

builder.Services.AddScoped<IProductImageStorage, ProductImageStorage>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

await IdentitySeeder.SeedRolesAndAdminAsync(app.Services, app.Configuration);

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
{
    app.UseExceptionHandler("/Home/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

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
