using System.Net;
using LocalServer.Server.Helpers;
using LocalServer.Server.Models;
using LocalServer.Server.Repository;

namespace LocalServer.Server.Services;

public class FileService
{
    private readonly string _saveDirPath = "C:\\Users\\gesefaki\\RiderProjects\\LocalServer\\data\\";

    private readonly FileRepository _db = new();

    public async Task<FileModel> CreateRequestModelFromHttpAsync(HttpListenerRequest request)
    {
        var fileModel = new FileModel
        {
            FileName = request.Headers["X-Original-Filename"]!,
            ContentType = request.Headers["X-Content-Type"] ?? "multipart/form-data",
            Size = request.ContentLength64,
            Method = request.HttpMethod,
            RawData = await ParseRawDataAsync(request.InputStream)
        };

        Console.WriteLine(
            $"[FILE_SERVICE_POST] File model created: " +
            $"{fileModel.FileName}, " +
            $"{fileModel.ContentType}, " +
            $"{fileModel.Size}, " +
            $"{fileModel.Method}");

        return fileModel;
    }

    public async Task<byte[]> ParseRawDataAsync(Stream inputStream)
    {
        using var ms = new MemoryStream();
        await inputStream.CopyToAsync(ms);
        return ms.ToArray();
    }

    public async Task UploadFileAsync(FileModel fileModel)
    {
        string savePath = _saveDirPath + fileModel.FileName;

        await File.WriteAllBytesAsync(savePath, fileModel.RawData);

        fileModel.SavePath = savePath;

        await _db.SaveFileAsync(fileModel);

        Console.WriteLine($"[FILE_SERVICE_OK] File saved in {savePath}");
    }

    public async Task<IEnumerable<FileResponse>> GetAllAsync()
    {
        var files = await _db.GetAllAsync();
        var result = files.Select(FileHelper.MapFileResponse);
        return result;
    }

    public async Task<BlobInfoResponse> GetBlobByIdAsync(int id)
    {
        var blobInfo = await _db.GetBlobInfoAsync(id);

        if (blobInfo == null)
        {
            throw new ArgumentException("File not found");
        }

        byte[] fileData = await File.ReadAllBytesAsync(blobInfo.SavePath);

        string contentType = blobInfo.ContentType;
        Stream stream = new MemoryStream(fileData);

        var result = new BlobInfoResponse(stream, contentType);

        return result;
    }

    public async Task DeleteAsync(int id)
    {
        var file = await _db.GetByIdAsync(id);

        if (file == null)
        {
            throw new ArgumentException("File not found");
        }

        try
        {
            bool isDeleted = await _db.TryDeleteAsync(id);
            if (isDeleted)
            {
                File.Delete(file.SavePath);
                Console.WriteLine($"[FILE_SERVICE_OK] File delete from {file.SavePath}");
            }
        }
        catch (Exception ex)
        {
            throw new FileNotFoundException("[FILE_SERVICE_ERROR]: " + ex.Message);
        }
    }
}