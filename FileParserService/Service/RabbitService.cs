using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace FileParserService.Service;

public class RabbitService : IHostedService, IAsyncDisposable
{
    private readonly string _queueName;
    private readonly ConnectionFactory _factory;
    private readonly ILogger<RabbitService> _logger;

    private IConnection? _connection;
    private IChannel? _channel;

    private const int MaxRetryAttempts = 10;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(3);
    
    private volatile bool _isConnected;

    public RabbitService(IConfiguration configuration, ILogger<RabbitService> logger)
    {
        var host = configuration["RabbitMq:Host"] 
            ?? throw new ArgumentNullException("RabbitMq:Host");
        var queueName = configuration["RabbitMq:QueueName"] 
            ?? throw new ArgumentNullException("RabbitMq:QueueName");
        var user = configuration["RabbitMq:User"] 
            ?? throw new ArgumentNullException("RabbitMq:User");
        var pass = configuration["RabbitMq:Pass"] 
            ?? throw new ArgumentNullException("RabbitMq:Pass");

        _queueName = queueName;
        _logger = logger;
        _factory = new ConnectionFactory
        {
            HostName = host,
            Port = 5672,
            UserName = user,
            Password = pass,
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true
        };
    }

    public async Task InitializeAsync(CancellationToken token = default)
    {
        for (int attempt = 1; attempt <= MaxRetryAttempts; attempt++)
        {
            try
            {
                _logger.LogInformation(
                    "Попытка подключения к RabbitMQ: {Attempt}/{MaxAttempts}",
                    attempt, MaxRetryAttempts);

                _connection = await _factory.CreateConnectionAsync(token);
                _channel = await _connection.CreateChannelAsync(cancellationToken: token);

                await _channel.QueueDeclareAsync(
                    queue: _queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    cancellationToken: token);
                
                _isConnected = true;

                _logger.LogInformation(
                    "Успешное подключение к RabbitMQ. Очередь: {QueueName}",
                    _queueName);
                
                return;
            }
            catch (Exception ex) when (attempt < MaxRetryAttempts)
            {
                _logger.LogWarning(
                    ex,
                    "Не удалось подключиться к RabbitMQ. Попытка {Attempt}/{MaxAttempts}.",
                    attempt, MaxRetryAttempts);

                await Task.Delay(RetryDelay, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Не удалось подключиться к RabbitMQ после {MaxAttempts} попыток",
                    MaxRetryAttempts);
                throw;
            }
        }
    }

    public async Task PublishAsync(byte[] message, CancellationToken token = default)
    {
        if (_channel is null || _connection?.IsOpen != true)
            throw new InvalidOperationException("RabbitService не инициализирован или соединение закрыто.");

        token.ThrowIfCancellationRequested();

        await _channel.BasicPublishAsync(
            exchange: "",
            routingKey: _queueName,
            body: message,
            cancellationToken: token
        );
    }

    public async ValueTask DisposeAsync()
    {
        _isConnected = false;
        
        if (_channel != null)
            await _channel.DisposeAsync();

        if (_connection != null)
            await _connection.DisposeAsync();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await InitializeAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _isConnected = false;
        await DisposeAsync();
    }
    
    public bool IsConnected => _isConnected;
}