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
            var fileName = $"{Guid.NewGuid():N}{extension}";

            var dir = Path.Combine(_env.WebRootPath, "uploads", "products");
            Directory.CreateDirectory(dir);

            var fullPath = Path.Combine(dir, fileName);

            await using var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await content.CopyToAsync(fs, cancellationToken);

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
