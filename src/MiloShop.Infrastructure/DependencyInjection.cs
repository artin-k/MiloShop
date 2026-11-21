using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiloShop.Infrastructure.Services;

namespace MiloShop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<MiloDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<ICouponService, CouponService>();
        services.AddScoped<MiloShop.Application.Catalog.ICatalogQueries, CatalogQueries>();
        services.AddScoped<ISiteSettingsService, SiteSettingsService>();

        return services;
    }
}
