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

    private readonly FileService _fileService = new();

    private const string Url = "http://localhost:5000/";

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

                if (request.Headers.AllKeys.Contains("X-Request-Type"))
                {
                    Console.WriteLine("[HTTP_SERVER_OK] Handling specific request...");
                    var fileModel = await _fileService.CreateRequestModelFromHttpAsync(request);
                    switch (fileModel.Method)
                    {
                        case "POST":
                            await _fileService.UploadFileAsync(fileModel);
                            break;

                        case "GET":
                            var (blob, files) = await HandleGetRequest(request);
                            if (blob != null)
                            {
                                byte[] buffer = await _fileService.ParseRawDataAsync(blob.Stream);

                                context.Response.ContentType = "application/octet-stream";
                                context.Response.ContentLength64 = blob.Stream.Length;
                                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                                context.Response.Close();

                                break;
                            }
                            else if (files != null)
                            {
                                string json = JsonSerializer.Serialize(files);
                                byte[] buffer = Encoding.UTF8.GetBytes(json);

                                context.Response.ContentType = "application/json";
                                context.Response.ContentLength64 = buffer.Length;
                                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                                context.Response.Close();

                                break;
                            }
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

    private async Task<(BlobInfoResponse? singleResponse, List<FileResponse>? multiplyResponse)> HandleGetRequest(HttpListenerRequest request)
    {
        int index = FileHelper.GetIndexFromRequest(request.RawUrl!);
        if (index > 0)
        {
            var file = await _fileService.GetBlobByIdAsync(index);
            return (file, null);
        }
        var files = await _fileService.GetAllAsync();
        return (null, (List<FileResponse>)files);
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