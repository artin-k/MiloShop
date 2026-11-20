using MiloShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MiloShop.Tests.Common;

internal static class TestDbContextFactory
{
    public static MiloDbContext Create(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<MiloDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString("N"))
            .Options;

        return new MiloDbContext(options);
    }
}
