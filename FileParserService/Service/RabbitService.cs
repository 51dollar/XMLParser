using RabbitMQ.Client;

namespace FileParserService.Service;

public class RabbitService : IAsyncDisposable
{
    private readonly string _queueName;
    private readonly ConnectionFactory _factory;

    private IConnection? _connection;
    private IChannel? _channel;
    
    public RabbitService(string host, string queueName)
    {
        _queueName = queueName;
        _factory = new ConnectionFactory
        {
            HostName = host,
            Port = 5672
        };
    }

    public async Task InitializeAsync(CancellationToken token = default)
    {
        _connection = await _factory.CreateConnectionAsync(token);
        _channel = await _connection.CreateChannelAsync(
            cancellationToken: token
        );

        await _channel.QueueDeclareAsync(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: token
        );
    }
    
    public async Task PublishAsync(byte[] message, CancellationToken token = default)
    {
        if (_channel is null)
            throw new InvalidOperationException("RabbitService не инициализирован. Вызовите InitializeAsync().");

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
        if (_channel != null)
            await _channel.DisposeAsync();

        if (_connection != null)
            await _connection.DisposeAsync();
    }
}