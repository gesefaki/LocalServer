using System.Net;
using System.Text;
using System.Text.Json;
using LocalServer.Server.Helpers;
using LocalServer.Server.Models;
using LocalServer.Server.Services;
using Microsoft.Extensions.Hosting;

namespace LocalServer.Server.Servers;

public class HttpServer : IHostedService, IDisposable
{
    private HttpListener? _httpListener;
    private CancellationTokenSource? _cts;
    private readonly IFileService _fileService;

    private const string Url = "http://localhost:5000/";

    public HttpServer(IFileService fileService)
    {
        _fileService = fileService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("[HTTP_SERVER_OK] Starting HTTP server...");

        _httpListener = new HttpListener();
        _httpListener.Start();
        _httpListener.Prefixes.Add(Url);

        _cts = new CancellationTokenSource();
        _ = Task.Run(() => ListenLoopAsync(_cts.Token), cancellationToken);
        return Task.CompletedTask;
    }

    private async Task ListenLoopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine($"[HTTP_SERVER_OK] HTTP server listening on {Url}");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var context = await _httpListener!.GetContextAsync();
                var request = context.Request;
                var response = context.Response;

                if (request.Headers.AllKeys.Contains("X-Request-Type"))
                {
                    Console.WriteLine("[HTTP_SERVER_OK] Handling specific request...");
                    var fileModel = await _fileService.CreateRequestModelFromHttpAsync(request);
                    switch (fileModel.Method)
                    {
                        case "POST":
                            await _fileService.UploadFileAsync(fileModel);

                            response.StatusCode = (int)HttpStatusCode.Created;
                            response.Close();
                            break;

                        case "GET":
                            var (blob, files) = await HandleGetRequest(request);
                            if (blob != null)
                            {
                                byte[] buffer = await _fileService.ParseRawDataAsync(blob.Stream);

                                response.ContentType = blob.ContentType;
                                response.ContentLength64 = blob.Stream.Length;
                                response.OutputStream.Write(buffer, 0, buffer.Length);
                                response.Close();

                                break;
                            }
                            else if (files != null)
                            {
                                string json = JsonSerializer.Serialize(files);
                                byte[] buffer = Encoding.UTF8.GetBytes(json);

                                response.ContentType = "application/json";
                                response.ContentLength64 = buffer.Length;
                                response.OutputStream.Write(buffer, 0, buffer.Length);
                                response.Close();

                                break;
                            }
                            break;
                        case "DELETE":
                            await HandleDeleteRequest(request);
                            response.StatusCode = (int)HttpStatusCode.NoContent;
                            response.Close();
                            break;
                        default:
                            continue;
                    }
                }
            }
        }
        catch (HttpListenerException ex) when (ex.ErrorCode == 995)
        {
            Console.WriteLine("[HTTP_SERVER_OK] Listener stopped normally.");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("[HTTP_SERVER_WARN] Operation cancelled.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("[HTTP_SERVER_ERROR] Error: " + ex.Message);
        }

    }

    private async Task<(BlobInfoResponse?, List<FileResponse>?)> HandleGetRequest(HttpListenerRequest request)
    {
        int index = FileHelper.GetIndexFromRequest(request.RawUrl!);
        if (index > 0)
        {
            var file = await _fileService.GetBlobByIdAsync(index);
            return (file, null);
        }
        var files = await _fileService.GetAllAsync();
        var filesList = files.ToList();
        return (null, filesList);
    }

    private async Task HandleDeleteRequest(HttpListenerRequest request)
    {
        int index = FileHelper.GetIndexFromRequest(request.RawUrl!);
        if (index > 0)
        {
            await _fileService.DeleteAsync(index);
        }
        return;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _httpListener?.Close();

        Dispose();

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();

        _httpListener?.Close();
        _httpListener = null;

        _cts = null;

        GC.SuppressFinalize(this);
    }
}