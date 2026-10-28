using System.Text;
using AccessRequestSystem.ProxyAgent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var logFilePath = builder.Configuration["Serilog:LogFilePath"] ?? "logs/proxy-agent-.log";
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Services.AddSerilog();

var apiBaseUrl = builder.Configuration["Api:BaseUrl"]
    ?? throw new InvalidOperationException("Api:BaseUrl не задан");

static HttpClientHandler CreateHandlerWithoutSystemProxy() => new()
{
    UseProxy = false,
    AutomaticDecompression = System.Net.DecompressionMethods.All
};

builder.Services.AddHttpClient<ApiAuthClient>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .ConfigurePrimaryHttpMessageHandler(CreateHandlerWithoutSystemProxy);
builder.Services.AddHttpClient<AccessCheckClient>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .ConfigurePrimaryHttpMessageHandler(CreateHandlerWithoutSystemProxy);
builder.Services.AddHttpClient<SsoTicketClient>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .ConfigurePrimaryHttpMessageHandler(CreateHandlerWithoutSystemProxy);

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ProxyAuthState>();
builder.Services.AddHostedService<ProxyHostedService>();
builder.Services.AddHostedService<ProxyControlHostedService>();

var host = builder.Build();

Log.Information("=== Proxy Agent запускается ===");
await host.RunAsync();