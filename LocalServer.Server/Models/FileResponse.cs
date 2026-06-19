namespace LocalServer.Server.Models;

public class FileResponse
{
    public int Id { get; init; }
    public string FileName { get; set; } = "";
    public string ContentType { get; set; } = "";
    public long Size { get; init; }
    public string SavePath { get; set; } = "";
}