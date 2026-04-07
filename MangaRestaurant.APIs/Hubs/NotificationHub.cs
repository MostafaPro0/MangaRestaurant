using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace MangaRestaurant.APIs.Hubs
{
    public class NotificationHub : Hub
    {
        // Hub logic can be added here if needed (e.g., joining groups)
        // For now, we mainly use it to push notifications from controllers/services.
        
        public async Task JoinUserGroup(string email)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, email);
        }

        public async Task LeaveUserGroup(string email)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, email);
        }
        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
        }

        public async Task LeaveAdminGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admins");
        }
    }
}
