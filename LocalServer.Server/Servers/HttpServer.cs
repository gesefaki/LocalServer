using System.Net;
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