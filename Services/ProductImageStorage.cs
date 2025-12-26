namespace avatCo.Services
{
    public class ProductImageStorage : IProductImageStorage
    {
        private readonly IWebHostEnvironment _env;

        public ProductImageStorage(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveAsync(IFormFile file, CancellationToken ct = default)
        {
            // file already validated by AllowedImageAttribute
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{ext}";

            var dir = Path.Combine(_env.WebRootPath, "uploads", "products");
            Directory.CreateDirectory(dir);

            var fullPath = Path.Combine(dir, fileName);

            await using var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await file.CopyToAsync(fs, ct);

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
