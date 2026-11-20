using MiloShop.Infrastructure.Persistence;
using MiloShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiloShop.Infrastructure.Services
{
    public class SiteSettingsService : ISiteSettingsService
    {
        private readonly MiloDbContext _db;
        public SiteSettingsService(MiloDbContext db) => _db = db;

        public async Task<string?> GetAsync(string key)
        {
            return await _db.SiteSettings
                .Where(x => x.Key == key)
                .Select(x => x.Value)
                .FirstOrDefaultAsync();
        }

        public async Task<Dictionary<string, string?>> GetManyAsync(params string[] keys)
        {
            var rows = await _db.SiteSettings
                .Where(x => keys.Contains(x.Key))
                .ToListAsync();

            return keys.ToDictionary(
                k => k,
                k => rows.FirstOrDefault(r => r.Key == k)?.Value
            );
        }

        public async Task SetAsync(string key, string? value)
        {
            var row = await _db.SiteSettings.FirstOrDefaultAsync(x => x.Key == key);
            if (row == null)
            {
                row = new SiteSetting { Key = key, Value = value, UpdatedAt = DateTime.UtcNow };
                _db.SiteSettings.Add(row);
            }
            else
            {
                row.Value = value;
                row.UpdatedAt = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync();
        }

        public async Task SetManyAsync(Dictionary<string, string?> values)
        {
            var keys = values.Keys.ToList();
            var existing = await _db.SiteSettings.Where(x => keys.Contains(x.Key)).ToListAsync();

            foreach (var kv in values)
            {
                var row = existing.FirstOrDefault(x => x.Key == kv.Key);
                if (row == null)
                {
                    _db.SiteSettings.Add(new SiteSetting
                    {
                        Key = kv.Key,
                        Value = kv.Value,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    row.Value = kv.Value;
                    row.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _db.SaveChangesAsync();
        }
    }
}
