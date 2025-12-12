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
var statusChanger = new StatusChangeService(logger);
var cts = new CancellationTokenSource();

var processedModel = new ProcessedModelService();
var models = xmlService.StartParse(cts.Token);

await Parallel.ForEachAsync(models, new ParallelOptions
    {
        MaxDegreeOfParallelism = 4, 
        CancellationToken = cts.Token
    },
    
    async (model, token) =>
    {
        var updated = await Task.Run(() => statusChanger.UpdateStatus(model), token);

        if (!updated)
            logger.LogError("Статус не обновлен: " + model.PackageID);
        else
            processedModel.AddInEnqueue(model);

        logger.LogInformation("Статус обновлен: " + model.PackageID);
    });

while (processedModel.TryGetNextModel(out var model))
{
    
}