using LocalServer.Server.Servers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<HttpServer>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<HttpServer>());

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
        policy.AllowAnyOrigin();
    });
});

builder.Services.AddHttpClient("DefaultClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:5000/");
    client.DefaultRequestHeaders.Add("X-Request-Type", "specific-request");
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Listen(System.Net.IPAddress.Any, 5144);
});

var app = builder.Build();

app.UseCors("AllowAll");

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();