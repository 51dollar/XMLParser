using System.Text;
using System.Text.Json;
using DataProcessorService.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Models.Parser.JSON;

namespace DataProcessorService.Messaging;

public class RabbitConsumer(
    IConfiguration config,
    ILogger<RabbitConsumer> logger,
    IServiceScopeFactory scopeFactory)
    : IHostedService, IAsyncDisposable
{
    private IConnection? _connection;
    private IChannel? _channel;
    
    private CancellationTokenSource? _processingCts;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var host = config["RabbitMq:Host"]
            ?? throw new ArgumentException("RabbitMq:Host нулевой");

        var queueName = config["RabbitMq:QueueName"]
            ?? throw new ArgumentException("RabbitMq:QueueName нулевой");

        var user = config["RabbitMq:User"]
            ?? throw new ArgumentException("RabbitMq:User нулевой");

        var pass = config["RabbitMq:Pass"]
            ?? throw new ArgumentException("RabbitMq:Pass нулевой");

        var factory = new ConnectionFactory
        {
            HostName = host,
            UserName = user,
            Password = pass,
            Port = AmqpTcpEndpoint.UseDefaultPort
        };

        const int maxRetries = 10;
        const int retryDelaySeconds = 3;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                logger.LogInformation("Попытка подключения к RabbitMQ: {Attempt}/{Max}", attempt, maxRetries);

                _connection = await factory.CreateConnectionAsync(cancellationToken);
                _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
                
                await _channel.BasicQosAsync(0, 1, false, cancellationToken);

                await _channel.QueueDeclareAsync(
                    queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    cancellationToken: cancellationToken);

                _processingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                
                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += (sender, args) => HandleMessageAsync(args, _processingCts.Token);

                await _channel.BasicConsumeAsync(
                    queue: queueName,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: cancellationToken);

                logger.LogInformation("Rabbit Consumer запущен. Очередь: {Queue}", queueName);
                return;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                logger.LogWarning(ex,
                    "Не удалось подключиться к RabbitMQ. Попытка {Attempt}.",
                    attempt);

                await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds), cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Не удалось подключиться к RabbitMQ после {Max} попыток", maxRetries);
                throw;
            }
        }
    }
    
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Остановка Rabbit Consumer");
        _processingCts?.Cancel();

        if (_channel != null)
            await _channel.CloseAsync(cancellationToken);

        if (_connection != null)
            await _connection.CloseAsync(cancellationToken);
        
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
        {
            await _channel.DisposeAsync();
            logger.LogInformation("Канал Rabbit Consumer закрыт");
        }

        if (_connection != null)
        {
            await _connection.DisposeAsync();
            logger.LogInformation("Соединение Rabbit Consumer закрыто");
        }
    }

    private async Task HandleMessageAsync(BasicDeliverEventArgs ea,
        CancellationToken token)
    {
        logger.LogInformation("Начало обработки сообщения.");
        try
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            
            var model = JsonSerializer.Deserialize<InstrumentStatusDto>(
                    body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? throw new InvalidOperationException("Ошибка десериализации json.");

            logger.LogInformation("Данные с сервиса получены! Id: {module}", model.PackageID);

            using var scope = scopeFactory.CreateScope();
            var sqliteService = scope.ServiceProvider.GetRequiredService<SqliteService>();
            
            foreach (var (moduleCategoryId, moduleState) in ExtractModuleData(model))
            {
                await sqliteService.AddDateAsync(
                    moduleCategoryId,
                    moduleState,
                    token);
            }

            await _channel!.BasicAckAsync(ea.DeliveryTag, false, token);
            logger.LogInformation("Сообщение успешно обработано");
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Обработка сообщения отменена");
        }
        catch (Exception ex)
        {
            await _channel!.BasicNackAsync(ea.DeliveryTag, false, true, token);
            logger.LogError(ex, "Ошибка обработки сообщения");
        }
    }

    private IEnumerable<(string ModuleCategoryID, string ModuleState)> ExtractModuleData(InstrumentStatusDto? model)
    {
        if (model?.DeviceStatus == null)
        {
            logger.LogWarning("Данные пустые!");
            yield break;
        }

        foreach (var device in model.DeviceStatus)
        {
            var moduleCategoryId = device.ModuleCategoryID;
            var moduleState = device.RapidControlStatus?.ModuleState;
            
            if (string.IsNullOrWhiteSpace(moduleCategoryId)
                || string.IsNullOrWhiteSpace(moduleState))
            {
                logger.LogWarning(
                    "Пропуск устройства: ModuleCategoryId = {ModuleCategoryId}, ModuleState = {ModuleState}",
                    moduleCategoryId,
                    moduleState);
                continue;
            }

            yield return (moduleCategoryId, moduleState);
        }
    }
}