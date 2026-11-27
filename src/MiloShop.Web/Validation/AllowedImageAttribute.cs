using System.ComponentModel.DataAnnotations;
using System.Xml;

namespace MiloShop.Web.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AllowedImageAttribute : ValidationAttribute
    {
        public long MaxBytes { get; set; } = 3 * 1024 * 1024;
        public bool AllowSvg { get; set; }

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
            if (ext == ".svg" && AllowSvg)
                return ValidateSvg(file);

            if (ext is not (".jpg" or ".jpeg" or ".png" or ".webp"))
                return new ValidationResult(AllowSvg
                    ? "فرمت‌های مجاز: SVG، JPG، PNG و WebP."
                    : "فرمت‌های مجاز: JPG، PNG و WebP.");

            if (!LooksLikeRealImage(file, ext))
                return new ValidationResult("محتوای تصویر با فرمت فایل مطابقت ندارد.");

            return ValidationResult.Success;
        }

        private static ValidationResult? ValidateSvg(IFormFile file)
        {
            if (file.Length > SvgIcon.MaxBytes)
                return new ValidationResult("حجم نماد SVG باید حداکثر ۱۲۸ کیلوبایت باشد.");

            try
            {
                using var stream = file.OpenReadStream();
                SvgIcon.Read(stream);
                return ValidationResult.Success;
            }
            catch (XmlException)
            {
                return new ValidationResult("یک SVG ساده با مسیرها و شکل‌ها بارگذاری کنید. اسکریپت، سبک CSS، تصویر جاسازی‌شده و پیوند مجاز نیست.");
            }
        }

        private static bool LooksLikeRealImage(IFormFile file, string extension)
        {
            try
            {
                using var stream = file.OpenReadStream();
                Span<byte> header = stackalloc byte[12];
                var read = stream.ReadAtLeast(header, header.Length, false);
                if (read < 12) return false;

                // JPEG: FF D8 FF
                if (extension is ".jpg" or ".jpeg" && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
                    return true;

                // PNG: 89 50 4E 47 0D 0A 1A 0A
                if (extension == ".png" && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                    header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
                    return true;

                // WEBP: "RIFF" .... "WEBP"
                // bytes 0-3 = RIFF, bytes 8-11 = WEBP
                if (extension == ".webp" && header[0] == (byte)'R' && header[1] == (byte)'I' && header[2] == (byte)'F' && header[3] == (byte)'F' &&
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
