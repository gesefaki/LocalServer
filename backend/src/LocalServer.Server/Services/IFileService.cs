using System.Net;
using LocalServer.Server.Models;

namespace LocalServer.Server.Services;

public interface IFileService
{
    Task<FileModel> CreateRequestModelFromHttpAsync(HttpListenerRequest request);
    Task<byte[]> ParseRawDataAsync(Stream inputStream);
    Task UploadFileAsync(FileModel fileModel);
    Task<IEnumerable<FileResponse>> GetAllAsync();
    Task<BlobInfoResponse> GetBlobByIdAsync(int id);
    Task DeleteAsync(int id);
}