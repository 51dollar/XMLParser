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

        while (!_rabbitService.IsConnected && !stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Ожидание подключения к RabbitMQ...");
            await Task.Delay(1000, stoppingToken);
        }

        _logger.LogInformation("RabbitMQ подключён, начинаем публикацию");

        while (!stoppingToken.IsCancellationRequested)
        {
            if (!_processedModel.TryGetNextModel(out var model))
            {
                await Task.Delay(500, stoppingToken);
                continue;
            }
            if (model == null)
            {
                _logger.LogWarning("Получена null модель");
                continue;
            }
            
            try
            {
                var jsonBytes = _jsonParser.ConvertToJson(model);
                _logger.LogInformation("Json преобразован!");

                await _rabbitService.PublishAsync(jsonBytes, stoppingToken);
                _logger.LogInformation("Json отправлен!");
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Операция отменена пользователем: {PackageId}", model?.PackageId);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка преобразования {PackageId}", model?.PackageId);
            }
        }

        _logger.LogInformation("Преобразователь json остановлен");
    }
}