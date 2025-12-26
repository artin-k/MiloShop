using System.ComponentModel.DataAnnotations;

namespace avatCo.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AllowedImageAttribute : ValidationAttribute
    {
        public long MaxBytes { get; set; } = 3 * 1024 * 1024;
        public string[] AllowedExtensions { get; set; } = Array.Empty<string>();

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null) return ValidationResult.Success;

            if (value is not IFormFile file)
                return new ValidationResult("Invalid file.");

            if (file.Length <= 0)
                return new ValidationResult("Empty file.");

            if (file.Length > MaxBytes)
                return new ValidationResult($"File too large. Max: {MaxBytes / (1024 * 1024)}MB");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (AllowedExtensions.Length > 0 && !AllowedExtensions.Contains(ext))
                return new ValidationResult($"Invalid file extension. Allowed: {string.Join(", ", AllowedExtensions)}");

            // ContentType check (extra layer)
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType))
                return new ValidationResult("Invalid image content type.");

            // MAGIC BYTES check (real safety)
            if (!LooksLikeRealImage(file))
                return new ValidationResult("File content is not a valid image.");

            return ValidationResult.Success;
        }

        private static bool LooksLikeRealImage(IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                Span<byte> header = stackalloc byte[12];
                var read = stream.Read(header);
                if (read < 12) return false;

                // JPEG: FF D8 FF
                if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
                    return true;

                // PNG: 89 50 4E 47 0D 0A 1A 0A
                if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                    header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
                    return true;

                // WEBP: "RIFF" .... "WEBP"
                // bytes 0-3 = RIFF, bytes 8-11 = WEBP
                if (header[0] == (byte)'R' && header[1] == (byte)'I' && header[2] == (byte)'F' && header[3] == (byte)'F' &&
                    header[8] == (byte)'W' && header[9] == (byte)'E' && header[10] == (byte)'B' && header[11] == (byte)'P')
                    return true;

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
