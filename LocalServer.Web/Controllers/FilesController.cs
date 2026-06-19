using LocalServer.Server.Models;
using LocalServer.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace LocalServer.Controllers;

[ApiController]
[Route("api/files")]

// http://localhost:5144/api/files
public class FilesController : ControllerBase
{
    private readonly string _url = "http://localhost:5000/";
    private readonly FileService _fileService = new();

    // POST: http://localhost:5144/api/files
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        using var http = new HttpClient();

        await using var stream = file.OpenReadStream();
        using var ms = new MemoryStream();

        await stream.CopyToAsync(ms, cancellationToken);
        byte[] data = ms.ToArray();

        HttpContent content = new ByteArrayContent(data);

        http.DefaultRequestHeaders.Add("X-Request-Type", "specific-request");
        http.DefaultRequestHeaders.Add("X-Content-Type", file.ContentType);
        http.DefaultRequestHeaders.Add("X-Original-Filename", file.FileName);
        http.DefaultRequestHeaders.Add("X-File-Size", file.Length.ToString());

        await http.PostAsync(_url, content, cancellationToken);

        return new CreatedResult(string.Empty, null);
    }

    // GET: http://localhost:5144/api/files
    [HttpGet]
    public async Task<ActionResult<List<FileResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _fileService.GetAllAsync();
        return Ok(result);
    }

    // GET: http://localhost:5144/api/files/{id}
    [HttpGet("{id::int}")]
    public async Task<IActionResult> GetFileById(int id, CancellationToken cancellationToken)
    {
        var blob = await _fileService.GetBlobByIdAsync(id);
        var file = File(blob.Stream, blob.ContentType);

        return file;
    }

    // DELETE: http://localhost:5144/api/files/{id}
    [HttpDelete("{id::int}")]
    public async Task<ActionResult> DeleteFile(int id, CancellationToken cancellationToken)
    {
        await _fileService.DeleteAsync(id);
        return NoContent();
    }
}