using Microsoft.AspNetCore.SignalR;

namespace MangaRestaurant.APIs.Hubs
{
    public class DeliveryHub : Hub
    {
        // Called by the customer's browser to watch a specific order
        public async Task JoinOrderGroup(string orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"order_{orderId}");
        }

        public async Task LeaveOrderGroup(string orderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order_{orderId}");
        }

        // Called by delivery agent to broadcast their location
        public async Task SendLocation(string orderId, double latitude, double longitude, string status)
        {
            await Clients.Group($"order_{orderId}").SendAsync("ReceiveLocation", new
            {
                OrderId   = orderId,
                Latitude  = latitude,
                Longitude = longitude,
                Status    = status,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
