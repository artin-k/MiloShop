namespace MiloShop.Application.Common.Interfaces;

public interface ISiteSettingsService
{
    Task<Dictionary<string, string?>> GetManyAsync(params string[] keys);
    Task<string?> GetAsync(string key);
    Task SetAsync(string key, string? value);
    Task SetManyAsync(Dictionary<string, string?> values);
}
