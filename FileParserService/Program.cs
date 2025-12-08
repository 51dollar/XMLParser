using FileParserService.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

string configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
builder.Services.AddSingleton(sp => new FolderPathService(
    sp.GetRequiredService<IConfiguration>(), sp.GetRequiredService<ILogger<FolderPathService>>(), configPath));

var host = builder.Build();

var folderService = host.Services.GetRequiredService<FolderPathService>();
var folderPath = await folderService.GetFolderPathAsync();