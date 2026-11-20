using avatCo.Services;
using avatCo.Tests.Common;
using FluentAssertions;

namespace avatCo.Tests.Services;

public class SiteSettingsServiceTests
{
    [Fact]
    public async Task SetManyAsync_ThenGetManyAsync_ReturnsSavedValues()
    {
        await using var db = TestDbContextFactory.Create();
        var sut = new SiteSettingsService(db);

        await sut.SetManyAsync(new Dictionary<string, string?>
        {
            ["MainPhone"] = "09120000000",
            ["SupportEmail"] = "support@example.com"
        });

        var values = await sut.GetManyAsync("MainPhone", "SupportEmail");

        values["MainPhone"].Should().Be("09120000000");
        values["SupportEmail"].Should().Be("support@example.com");
    }

    [Fact]
    public async Task SetAsync_ExistingKey_UpdatesValue()
    {
        await using var db = TestDbContextFactory.Create();
        var sut = new SiteSettingsService(db);

        await sut.SetAsync("TwitterUrl", "https://x.com/old");
        await sut.SetAsync("TwitterUrl", "https://x.com/new");

        var value = await sut.GetAsync("TwitterUrl");

        value.Should().Be("https://x.com/new");
    }
}
