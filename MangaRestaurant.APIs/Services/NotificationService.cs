using MangaRestaurant.APIs.Hubs;
using MangaRestaurant.Core.Service;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace MangaRestaurant.APIs.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendOrderStatusUpdate(string email, string orderId, string status)
        {
            await _hubContext.Clients.Group(email).SendAsync("ReceiveNotification", new
            {
                Title = "Order Update",
                TitleAr = "تحديث الطلب",
                Message = $"Order #{orderId} status is now: {status}",
                MessageAr = $"طلبك رقم #{orderId} أصبح الآن: {status}",
                Type = "OrderUpdate"
            });
        }

        public async Task SendNewProductNotification(string productName)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
            {
                Title = "New Item Added!",
                TitleAr = "صنف جديد!",
                Message = $"Check out our new {productName} deliciousness!",
                MessageAr = $"لقد أضفنا {productName} الجديد، جربه الآن!",
                Type = "NewProduct"
            });
        }

        public async Task NotifyAdminNewOrder(string orderId, decimal total)
        {
            await _hubContext.Clients.Group("Admins").SendAsync("ReceiveAdminNotification", new
            {
                Title = "New Order!",
                Message = $"Order #{orderId} received. Total: {total:c}",
                Type = "NewOrder"
            });
        }

        public async Task NotifyAdminNewReview(string productName, string userName, int rating)
        {
            await _hubContext.Clients.Group("Admins").SendAsync("ReceiveAdminNotification", new
            {
                Title = "New Review",
                Message = $"{userName} rated {productName} with {rating} stars.",
                Type = "NewReview"
            });
        }
    }
}
