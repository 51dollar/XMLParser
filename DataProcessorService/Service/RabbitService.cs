using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Models;

namespace DataProcessorService.Service;

public class RabbitService(IConfiguration config) : BackgroundService
{
    private readonly ILogger<RabbitService> _logger;
    private IConnection? _connection;
    private IChannel? _channel;
    
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var queueName = config["RabbitMq:QueueName"]
            ?? throw new ArgumentException("RabbitMq:Host не должен быть null");
        
        var factory = new ConnectionFactory
        {
            HostName = config["RabbitMq:Host"] 
                ?? throw new ArgumentException("RabbitMq:Host не должен быть null")
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
        
        await _channel.QueueDeclareAsync(
            queueName,
            true,
            false,
            false,
            cancellationToken: cancellationToken);
        
        var eventConsumer = new AsyncEventingBasicConsumer(_channel);
        eventConsumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());

                var model = JsonSerializer.Deserialize<ModelXmlParse>(
                    body,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (model == null)
                    throw new InvalidOperationException("Не удалось десериализовать JSON");

                _logger.LogInformation(
                    "Сообщение пришло!\nPackageID: {PackageId}, DeviceStatus: {Count}",
                    model.PackageID,
                    model.DeviceStatus?.Length ?? 0
                );

                await _channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обработки JSON");
                await _channel!.BasicNackAsync(ea.DeliveryTag, false, true, cancellationToken);
            }

            await _channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: eventConsumer,
                cancellationToken: cancellationToken
            );

            await base.StartAsync(cancellationToken);
        };
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
    
    public override void Dispose()
    {
        _channel?.CloseAsync();
        _connection?.CloseAsync();
        base.Dispose();
    }
}