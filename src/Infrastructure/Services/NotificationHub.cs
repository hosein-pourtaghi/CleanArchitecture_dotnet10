using Microsoft.AspNetCore.SignalR;
namespace Infrastructure.Services;
public class NotificationHub : Hub
{
    // Server can call Clients.User(userId).SendAsync("OrderCreated", payload)
}
