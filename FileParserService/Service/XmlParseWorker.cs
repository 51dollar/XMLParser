using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FileParserService.Service;

public class XmlParseWorker(
    ILogger<XmlParseWorker> logger,
    ILoggerFactory loggerFactory,
    FolderPathService folderPathService,
    StatusChangeService statusChanger,
    ProcessedModelService processedModel)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Преобразователь xml запущен!");
        
        var folderPath = await folderPathService.GetFolderPathAsync();
        var xmlLogger = loggerFactory.CreateLogger<XmlParseService>();
        var xmlService = new XmlParseService(folderPath, xmlLogger);
        var models = xmlService.StartParse(stoppingToken);

        await Parallel.ForEachAsync(
            models,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = 4,
                CancellationToken = stoppingToken
            },
            async (model, token) =>
            {
                try
                {
                    var updated = await Task.Run(
                        () => statusChanger.UpdateStatus(model),
                        token);

                    if (!updated)
                    {
                        logger.LogError("Статус не обновлен: {PackageId}", model.PackageId);
                    }
                    else
                    {
                        processedModel.AddInEnqueue(model);
                        logger.LogInformation("Статус обновлен: {PackageId}", model.PackageId);
                    }
                }
                catch (OperationCanceledException)
                {
                    logger.LogWarning("Отмена обработки модели: {PackageId}", model.PackageId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Ошибка обработки модели: {PackageId}", model.PackageId);
                }
            });

        logger.LogInformation("Преобразователь xml завершён!");
    }
}