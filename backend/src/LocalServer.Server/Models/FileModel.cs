namespace LocalServer.Server.Models;

public class FileModel
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public long Size { get; set; }
    public string SavePath { get; set; } = string.Empty;
    public byte[] RawData { get; set; } = [];

    public override string ToString()
    {
        return $"Id: {Id}, FileName: {FileName}, ContentType: {ContentType}, Size: {Size}, SavePath: {SavePath}";
    }
}