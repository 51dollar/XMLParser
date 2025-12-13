using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FileParserService.Service;

public class XmlParseWorker : BackgroundService
{
    private readonly ILogger<XmlParseWorker> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly FolderPathService _folderPathService;
    private readonly StatusChangeService _statusChanger;
    private readonly ProcessedModelService _processedModel;

    public XmlParseWorker(
        ILogger<XmlParseWorker> logger,
        ILoggerFactory loggerFactory,
        FolderPathService folderPathService,
        StatusChangeService statusChanger,
        ProcessedModelService processedModel)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _folderPathService  = folderPathService;
        _statusChanger = statusChanger;
        _processedModel = processedModel;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Преобразователь xml запущен!");
        
        var folderPath = await _folderPathService.GetFolderPathAsync();
        var xmlLogger = _loggerFactory.CreateLogger<XmlParseService>();
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
                        () => _statusChanger.UpdateStatus(model),
                        token);

                    if (!updated)
                    {
                        _logger.LogError("Статус не обновлен: {PackageId}", model.PackageID);
                    }
                    else
                    {
                        _processedModel.AddInEnqueue(model);
                        _logger.LogInformation("Статус обновлен: {PackageId}", model.PackageID);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Отмена обработки модели: {PackageId}", model.PackageID);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка обработки модели: {PackageId}", model.PackageID);
                }
            });

        _logger.LogInformation("Преобразователь xml завершён!");
    }
}