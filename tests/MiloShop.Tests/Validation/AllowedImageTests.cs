using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Http;
using MiloShop.Web.Validation;

namespace MiloShop.Tests.Validation;

public class AllowedImageTests
{
    [Theory]
    [InlineData("icon.svg", "<path d='M0 0L24 24' stroke='currentColor'/>", true)]
    [InlineData("icon.svg", "<defs><linearGradient id='ink'/></defs><path fill='url(#ink)'/>", true)]
    [InlineData("icon.svg", "<script/>", false)]
    [InlineData("icon.svg", "<path onload='alert(1)'/>", false)]
    [InlineData("icon.svg", "<path fill='url(https://example.test/icon.svg#ink)'/>", false)]
    [InlineData("icon.svg", "<image href='https://example.test/icon.png'/>", false)]
    [InlineData("icon.svg", "<?xml-stylesheet href='external.css'?>", false)]
    [InlineData("icon.png", "<path/>", false)]
    public void Validate_RequiresStaticSvgWithMatchingExtension(string fileName, string body, bool accepted)
    {
        var bytes = Encoding.UTF8.GetBytes($"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'>{body}</svg>");
        using var stream = new MemoryStream(bytes);
        var file = new FormFile(stream, 0, bytes.Length, "ImageFile", fileName);
        var validator = new AllowedImageAttribute { AllowSvg = true };

        var result = validator.GetValidationResult(file, new ValidationContext(file));
        Assert.Equal(accepted, result == ValidationResult.Success);
    }
}
