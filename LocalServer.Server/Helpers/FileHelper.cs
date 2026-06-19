using LocalServer.Server.Models;

namespace LocalServer.Server.Helpers;

public static class FileHelper
{
    public static FileResponse MapFileResponse(this FileModel file)
    {
        return new FileResponse
        {
            Id = file.Id,
            ContentType = file.ContentType,
            FileName = file.FileName,
            SavePath = file.SavePath,
            Size = file.Size
        };
    }
}