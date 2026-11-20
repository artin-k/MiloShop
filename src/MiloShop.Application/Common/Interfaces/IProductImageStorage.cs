namespace MiloShop.Application.Common.Interfaces;

public interface IProductImageStorage
{
    Task<string> SaveAsync(
        Stream content,
        string fileExtension,
        CancellationToken cancellationToken = default);

    void DeleteIfExists(string? imageUrl);
}
