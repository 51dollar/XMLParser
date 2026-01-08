using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Models.Parser.JSON;

namespace DataProcessorService.Service;

public class RabbitService(
    IConfiguration config,
    ILogger<RabbitService> logger,
    IServiceScopeFactory scopeFactory)
    : IHostedService, IAsyncDisposable
{
    private IConnection? _connection;
    private IChannel? _channel;

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

                await _channel.QueueDeclareAsync(
                    queueName,
                    true,
                    false,
                    false,
                    cancellationToken: cancellationToken);

                var eventConsumer = new AsyncEventingBasicConsumer(_channel);
                eventConsumer.ReceivedAsync += HandleMessageAsync;

                await _channel.BasicConsumeAsync(
                    queue: queueName,
                    autoAck: false,
                    consumer: eventConsumer,
                    cancellationToken: cancellationToken
                );

                logger.LogInformation("RabbitMQ запущен. Очередь: {Queue}", queueName);
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
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("RabbitMQ останавлин.");
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
        {
            await _channel.CloseAsync();
            logger.LogInformation("Канал RabbitMQ закрыт");
        }

        if (_connection != null)
        {
            await _connection.CloseAsync();
            logger.LogInformation("Соединение RabbitMQ закрыто");
        }
    }

    private async Task HandleMessageAsync(object sender, BasicDeliverEventArgs ea)
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
                    CancellationToken.None);
            }

            await _channel!.BasicAckAsync(ea.DeliveryTag, false);
            logger.LogInformation("Сообщение успешно обработано");
        }
        catch (Exception ex)
        {
            await _channel!.BasicNackAsync(ea.DeliveryTag, false, false);
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