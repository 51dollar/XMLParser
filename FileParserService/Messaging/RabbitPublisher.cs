using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace FileParserService.Messaging;

public class RabbitPublisher : IHostedService, IAsyncDisposable
{
    private readonly string _queueName;
    private readonly ConnectionFactory _factory;
    private readonly ILogger<RabbitPublisher> _logger;

    private IConnection? _connection;
    private IChannel? _channel;
    private readonly SemaphoreSlim _publishLock = new(1, 1);

    private const int MaxRetryAttempts = 10;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(3);

    public RabbitPublisher(IConfiguration configuration, ILogger<RabbitPublisher> logger)
    {
        _logger = logger;

        var host = configuration["RabbitMq:Host"]
            ?? throw new ArgumentNullException("RabbitMq:Host");
        _queueName = configuration["RabbitMq:QueueName"]
            ?? throw new ArgumentNullException("RabbitMq:QueueName");

        _factory = new ConnectionFactory
        {
            HostName = host,
            Port = 5672,
            UserName = configuration["RabbitMq:User"],
            Password = configuration["RabbitMq:Pass"],
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true
        };
    }
    
    public bool IsConnected =>
        _connection?.IsOpen == true &&
        _channel?.IsOpen == true;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await InitializeAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await DisposeAsync();
    }
    
    private async Task InitializeAsync(CancellationToken token = default)
    {
        for (int attempt = 1; attempt <= MaxRetryAttempts; attempt++)
        {
            try
            {
                _logger.LogInformation("Попытка подключения к RabbitMQ: {Attempt}/{MaxAttempts}",
                    attempt, MaxRetryAttempts);

                _connection = await _factory.CreateConnectionAsync(token);
                _channel = await _connection.CreateChannelAsync(cancellationToken: token);

                await _channel.QueueDeclareAsync(
                    queue: _queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    cancellationToken: token);

                _logger.LogInformation("Успешное подключение к RabbitMQ. Очередь: {QueueName}",
                    _queueName);
                
                return;
            }
            catch (Exception ex) when (attempt < MaxRetryAttempts)
            {
                _logger.LogWarning(ex, "Ошибка подключения к RabbitMQ ({Attempt}/{Max}). Повтор...",
                    attempt, MaxRetryAttempts);

                await Task.Delay(RetryDelay, token);
            }
        }
        
        throw new InvalidOperationException("Не удалось подключиться к RabbitMQ");
    }

    public async Task PublishAsync(byte[] message, CancellationToken token = default)
    {
        if (!IsConnected)
        {
            _logger.LogWarning("Соединение с RabbitMQ потеряно. Попытка переподключения.");
            await InitializeAsync(token);
        }
        
        await _publishLock.WaitAsync(token);
        try
        {
            if (!IsConnected)
                throw new InvalidOperationException("RabbitMQ недоступен");
            
            await _channel!.BasicPublishAsync(
                exchange: "",
                routingKey: _queueName,
                body: message,
                cancellationToken: token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка публикации сообщения в RabbitMQ");
            throw;
        }
        finally
        {
            _publishLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_channel != null)
                await _channel.DisposeAsync();

            if (_connection != null)
                await _connection.DisposeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при закрытии RabbitMQ");
        }
    }
}