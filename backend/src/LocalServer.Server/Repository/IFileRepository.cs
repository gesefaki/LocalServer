using LocalServer.Server.Models;

namespace LocalServer.Server.Repository;

public interface IFileRepository
{
    Task<int> SaveFileAsync(FileModel fileModel);
    Task<IEnumerable<FileModel>> GetAllAsync();
    Task<BlobInfo> GetBlobInfoAsync(int id);
    Task<FileModel?> GetByIdAsync(int id);
    Task<bool> TryDeleteAsync(int id);
}