using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FileParserService.Service;

public class JsonPublishWorker : BackgroundService
{
    private readonly ILogger<JsonPublishWorker> _logger;
    private readonly RabbitService _rabbitService;
    private readonly JsonParserService _jsonParser;
    private readonly ProcessedModelService _processedModel;

    public JsonPublishWorker(
        ILogger<JsonPublishWorker> logger,
        RabbitService rabbitService,
        JsonParserService jsonParser,
        ProcessedModelService processedModel)
    {
        _logger = logger;
        _rabbitService = rabbitService;
        _jsonParser = jsonParser;
        _processedModel = processedModel;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Преобразователь json запущен");

        await _rabbitService.InitializeAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested &&
               _processedModel.TryGetNextModel(out var model))
        {
            _logger.LogInformation("Преобразовываем данные в json: {PackageId}", model.PackageID);

            try
            {
                var jsonBytes = _jsonParser.ConvertToJson(model);
                _logger.LogInformation("Json преобразован.");

                await _rabbitService.PublishAsync(jsonBytes, stoppingToken);
                _logger.LogInformation("Json отправлен!");
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Операция отменена пользователем: {PackageId}", model.PackageID);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка преобразования {PackageId}", model.PackageID);
            }
        }

        _logger.LogInformation("Преобразователь json остановлен");
    }
}