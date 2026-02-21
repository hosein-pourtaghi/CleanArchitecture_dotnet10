using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;


namespace Infrastructure.Services;

public interface IMessagePublisher
{
    void Publish(string exchange, string routingKey, byte[] body);
}

//  
//
// public class RabbitMqPublisher : IMessagePublisher, IAsyncDisposable
// {
//     private readonly IConnection _connection;
//     private readonly IModel _channel;
//
//     public RabbitMqPublisher(IConfiguration cfg)
//     {
//         var factory = new ConnectionFactory
//         {
//             HostName = cfg["Rabbit:Host"] ?? "localhost"
//         };
//
//         // Use Async connection
//         _connection = factory.CreateConnection(); // still works in 7.3 with net10
//         _channel = _connection.CreateModel();
//     }
//
//     public void Publish(string exchange, string routingKey, byte[] body)
//     {
//         _channel.BasicPublish(exchange: exchange,
//             routingKey: routingKey,
//             basicProperties: null,
//             body: body);
//     }
//
//     public async ValueTask DisposeAsync()
//     {
//         _channel?.Close();
//         _channel?.Dispose();
//         _connection?.Close();
//         _connection?.Dispose();
//         await Task.CompletedTask;
//     }
// }
