using FileParserService.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.ClearProviders();
    builder.AddConsole();
});

ILogger logger = loggerFactory.CreateLogger("AppLogger");

string configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
var folderService = new FolderPathService(configuration, logger, configPath);
var folderPath = await folderService.GetFolderPathAsync();

var xmlService = new XmlParseService(folderPath, logger);
var cts = new CancellationTokenSource();
await foreach (var model in xmlService.StartParse(cts.Token))
{
    logger.LogInformation("Обработка модели: " + model);
}