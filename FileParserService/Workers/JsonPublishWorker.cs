using FileParserService.Messaging;
using FileParserService.Parsing;
using FileParserService.Processing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FileParserService.Workers;

public class JsonPublishWorker : BackgroundService
{
    private readonly ILogger<JsonPublishWorker> _logger;
    private readonly RabbitPublisher _rabbitPublisher;
    private readonly JsonMessageSerializer _jsonMessageParser;
    private readonly ProcessedModelHandler _processedModel;

    public JsonPublishWorker(
        ILogger<JsonPublishWorker> logger,
        RabbitPublisher rabbitPublisher,
        JsonMessageSerializer jsonMessageParser,
        ProcessedModelHandler processedModel)
    {
        _logger = logger;
        _rabbitPublisher = rabbitPublisher;
        _jsonMessageParser = jsonMessageParser;
        _processedModel = processedModel;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Преобразователь json запущен");

        while (!_rabbitPublisher.IsConnected && !stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Ожидание подключения к RabbitMQ...");
            await Task.Delay(1000, stoppingToken);
        }

        _logger.LogInformation("RabbitMQ подключён, начинаем публикацию");

        while (!stoppingToken.IsCancellationRequested)
        {
            if (!_processedModel.TryGetNextModel(out var model) || model == null)
            {
                await Task.Delay(500, stoppingToken);
                continue;
            }
            
            try
            {
                var jsonBytes = _jsonMessageParser.ConvertToJson(model);
                _logger.LogDebug("Json преобразован для {PackageId}", model.PackageId);
                await _rabbitPublisher.PublishAsync(jsonBytes, stoppingToken);
                _logger.LogInformation("Пакет {PackageId} отправлен", model.PackageId);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Операция отменена пользователем: {PackageId}", model.PackageId);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка преобразования {PackageId}", model.PackageId);
                _processedModel.AddInEnqueue(model);
            }
        }

        _logger.LogInformation("Преобразователь json остановлен");
    }
}