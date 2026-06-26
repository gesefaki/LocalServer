using System.Text.Json;
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
    private readonly IHttpClientFactory _http;

    public FilesController(IHttpClientFactory http)
    {
        _http = http;
    }

    // POST: http://localhost:5144/api/files
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        using var http = _http.CreateClient("DefaultClient");

        await using var stream = file.OpenReadStream();
        using var ms = new MemoryStream();

        await stream.CopyToAsync(ms, cancellationToken);
        byte[] data = ms.ToArray();

        HttpContent content = new ByteArrayContent(data);

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
        using var http = _http.CreateClient("DefaultClient");

        http.DefaultRequestHeaders.Add("X-Request-Type", "specific-request");
        var response = await http.GetAsync(_url, cancellationToken);

        var json = await response.Content.ReadAsStringAsync();
        return Ok(JsonSerializer.Deserialize<List<FileResponse>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<FileResponse>());
    }

    // GET: http://localhost:5144/api/files/{id}
    [HttpGet("{id::int}")]
    public async Task<IActionResult> GetFileById(int id, CancellationToken cancellationToken)
    {
        using var http = _http.CreateClient("DefaultClient");

        http.DefaultRequestHeaders.Add("X-Request-Type", "specific-request");
        var url = _url + $"/{id}";
        var response = await http.GetAsync(url, cancellationToken);

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

        return File(bytes, response.Content.Headers.ContentType!.MediaType!);
    }

    // DELETE: http://localhost:5144/api/files/{id}
    [HttpDelete("{id::int}")]
    public async Task<ActionResult> DeleteFile(int id, CancellationToken cancellationToken)
    {
        using var http = _http.CreateClient("DefaultClient");

        http.DefaultRequestHeaders.Add("X-Request-Type", "specific-request");
        var url = _url + $"/{id}";
        var response = await http.DeleteAsync(url, cancellationToken);

        return NoContent();
    }
}