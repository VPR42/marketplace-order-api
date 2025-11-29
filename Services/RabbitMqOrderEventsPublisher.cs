using System.Text;
using System.Text.Json;
using MarketPlace.Models;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace MarketPlace.Services;

public class RabbitMqOrderEventsPublisher : IOrderEventsPublisher, IDisposable
{
    private readonly RabbitMqSettings _settings;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqOrderEventsPublisher(IOptions<RabbitMqSettings> options)
    {
        _settings = options.Value;

        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(exchange: _settings.Exchange, type: ExchangeType.Topic, durable: true);
    }

    public Task PublishOrderCreatedAsync(Order order)
    {
        var message = new
        {
            type = "order_created",
            orderId = order.Id,
            userId = order.UserId,
            jobId = order.JobId,
            status = order.Status,
            orderedAt = order.OrderedAt
        };

        Publish("order.created", message);
        return Task.CompletedTask;
    }

    public Task PublishOrderClosedAsync(Order order)
    {
        var message = new
        {
            type = "order_closed",
            orderId = order.Id,
            userId = order.UserId,
            jobId = order.JobId,
            status = order.Status,
            closedAt = DateTime.UtcNow
        };

        Publish("order.closed", message);
        return Task.CompletedTask;
    }

    private void Publish(string routingKey, object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        var body = Encoding.UTF8.GetBytes(json);

        _channel.BasicPublish(
            exchange: _settings.Exchange,
            routingKey: routingKey,
            basicProperties: null,
            body: body);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}