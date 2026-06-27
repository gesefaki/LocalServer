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

    public static int GetIndexFromRequest(string url)
    {
        if (url.Length > 0 && char.IsDigit(url[^1]))
        {
            var splittedUrl = url.Split('/');
            int.TryParse(splittedUrl[^1], out var index);
            return index;
        }
        return -1;
    }

}