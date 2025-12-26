namespace avatCo.Services
{
    public interface IProductImageStorage
    {
        Task<string> SaveAsync(IFormFile file, CancellationToken ct = default);
        void DeleteIfExists(string? imageUrl);
    }
}
