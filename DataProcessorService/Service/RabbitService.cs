using System.Text;
using System.Text.Json;
using DataProcessorService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Models;

namespace DataProcessorService.Service;

public class RabbitService(IConfiguration config, ILogger<RabbitService> logger, SqliteService sqliteService)
    : BackgroundService
{
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
        eventConsumer.ReceivedAsync += HandleMessageAsync;

        await _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: eventConsumer,
            cancellationToken: cancellationToken
        );

        await base.StartAsync(cancellationToken);
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

    private async Task HandleMessageAsync(object sender, BasicDeliverEventArgs ea)
    {
        try
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            var model = JsonSerializer.Deserialize<ModelXmlParse>(body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidOperationException("Ошибка десериализации json.");

            var moduleDataList = ExtractModuleData(model);

            foreach (var module in moduleDataList)
            {
                await sqliteService.AddDateAsync(
                    module.ModuleCategoryID,
                    module.ModuleState,
                    CancellationToken.None);
            }

            await _channel!.BasicAckAsync(ea.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка обработки сообщения");
            await _channel!.BasicNackAsync(ea.DeliveryTag, false, true);
        }
    }

    private static IReadOnlyCollection<ModuleData> ExtractModuleData(ModelXmlParse model)
    {
        return model.DeviceStatus
            .SelectMany(
                device => device.RapidControlStatus,
                (device, rapid) => new { device, rapid })
            .SelectMany(
                x => x.rapid.CombinedSamplerStatus,
                (x, combined) => new { x.device.ModuleCategoryID, combined.ModuleState })
            .Where(x => int.TryParse(x.ModuleCategoryID, out _))
            .Select(x => new ModuleData
            {
                ModuleCategoryID = int.Parse(x.ModuleCategoryID),
                ModuleState = x.ModuleState
            })
            .ToList();
    }
}