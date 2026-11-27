using System.Xml.Linq;
using MiloShop.Web.Validation;

namespace MiloShop.Web.Services
{
    public class ProductImageStorage : IProductImageStorage
    {
        private readonly IWebHostEnvironment _env;

        public ProductImageStorage(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveAsync(
            Stream content,
            string fileExtension,
            CancellationToken cancellationToken = default)
        {
            var extension = fileExtension.ToLowerInvariant();
            var svg = extension == ".svg" ? SvgIcon.Read(content) : null;
            var fileName = $"{Guid.NewGuid():N}{extension}";

            var dir = Path.Combine(_env.WebRootPath, "uploads", "products");
            Directory.CreateDirectory(dir);

            var fullPath = Path.Combine(dir, fileName);

            await using var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            if (svg is null)
                await content.CopyToAsync(fs, cancellationToken);
            else
                await svg.SaveAsync(fs, SaveOptions.DisableFormatting, cancellationToken);

            return $"/uploads/products/{fileName}";
        }

        public void DeleteIfExists(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl)) return;

            var rel = imageUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());
            var full = Path.Combine(_env.WebRootPath, rel);

            if (File.Exists(full))
                File.Delete(full);
        }
    }
}
