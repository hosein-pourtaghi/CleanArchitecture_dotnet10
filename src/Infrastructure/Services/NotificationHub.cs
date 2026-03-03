using System.Diagnostics;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Services;
 
/// <summary>
/// Handles real-time notification broadcasting using SignalR.
/// Follows Microsoft's standard trace context propagation.
/// </summary>
public class NotificationHub : Hub
{
    
    // Server can call Clients.User(userId).SendAsync("CartCreated", payload)
    //public async Task SendNotification(string message)
    //{
    //    // Use standard Activity.Current.Id for trace context
    //    var traceId = Activity.Current?.Id ?? "N/A";

    //    // Broadcast to all connected clients
    //    await Clients.All.SendAsync("ReceiveNotification", message, traceId);
    //}
}
