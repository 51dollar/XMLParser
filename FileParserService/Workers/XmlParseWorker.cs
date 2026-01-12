using FileParserService.Parsing;
using FileParserService.Processing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FileParserService.Workers;

public class XmlParseWorker(
    ILogger<XmlParseWorker> logger,
    XmlParser xmlParser,
    StatusChangeProcessor statusChanger,
    ProcessedModelHandler processedModel)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Преобразователь xml запущен!");
            
            var models = xmlParser.StartParseAsync(stoppingToken);

            await Parallel.ForEachAsync(
                models,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = 4,
                    CancellationToken = stoppingToken
                }, (model, token) =>
                {
                    try
                    {
                        var updated = statusChanger.UpdateStatus(model, token);
                        if (!updated)
                        {
                            logger.LogWarning("Статус не обновлен: {PackageId}", model.PackageId);
                        }

                        processedModel.AddInEnqueue(model);
                        logger.LogInformation("Статус обновлен: {PackageId}", model.PackageId);
                    }
                    catch (OperationCanceledException)
                    {
                        logger.LogWarning("Отмена обработки модели: {PackageId}", model.PackageId);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Ошибка обработки модели: {PackageId}", model.PackageId);
                    }

                    return ValueTask.CompletedTask;
                });
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }

        logger.LogInformation("Преобразователь xml завершён!");
    }
}