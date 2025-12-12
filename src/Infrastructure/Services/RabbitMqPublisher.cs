using System.Text;
using RabbitMQ.Client;
namespace Infrastructure.Services;
public interface IMessagePublisher { void Publish(string exchange, string routingKey, byte[] body); }
public class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    private readonly IConnection _conn;
    private readonly IModel _ch;
    public RabbitMqPublisher(IConfiguration cfg)
    {
        var factory = new ConnectionFactory(){ HostName = cfg["Rabbit:Host"] ?? "localhost" };
        _conn = factory.CreateConnection();
        _ch = _conn.CreateModel();
    }
    public void Publish(string exchange, string routingKey, byte[] body)
    {
        _ch.BasicPublish(exchange: exchange, routingKey: routingKey, basicProperties: null, body: body);
    }
    public void Dispose(){ _ch?.Dispose(); _conn?.Dispose(); }
}
