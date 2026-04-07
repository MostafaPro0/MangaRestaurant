using MangaRestaurant.APIs.Hubs;
using MangaRestaurant.Core;
using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.RepositoriesContract;
using MangaRestaurant.Core.Service;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace MangaRestaurant.APIs.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(IHubContext<NotificationHub> hubContext, IUnitOfWork unitOfWork)
        {
            _hubContext = hubContext;
            _unitOfWork = unitOfWork;
        }

        private async Task SaveNotificationAsync(string title, string titleAr, string message, string messageAr, NotificationType type, string targetUser, string relatedId = null)
        {
            var notification = new Notification
            {
                Title = title,
                TitleAr = titleAr,
                Message = message,
                MessageAr = messageAr,
                Type = type,
                TargetUser = targetUser,
                RelatedId = relatedId
            };

            await _unitOfWork.Repository<Notification>().AddAsync(notification);
            await _unitOfWork.CompleteAsync();
        }

        public async Task SendOrderStatusUpdate(string email, string orderId, string status)
        {
            var title = "Order Update";
            var titleAr = "تحديث الطلب";
            var message = $"Order #{orderId} status is now: {status}";
            
            string statusAr = status switch
            {
                "Pending" => "قيد الانتظار",
                "PaymentReceived" => "تم الدفع",
                "Payment Received" => "تم الدفع",
                "PaymentFailed" => "فشل الدفع",
                "Payment Failed" => "فشل الدفع",
                "Confirmed" => "تم التأكيد",
                "Processing" => "قيد التحضير",
                "Shipped" => "تم الشحن",
                "Delivered" => "تم التوصيل",
                "Completed" => "مكتمل",
                "Cancelled" => "ملغي",
                "Refunded" => "مسترجع",
                _ => status
            };

            var messageAr = $"طلبك رقم #{orderId} أصبح الآن: {statusAr}";

            await SaveNotificationAsync(title, titleAr, message, messageAr, NotificationType.OrderUpdate, email, orderId);

            await _hubContext.Clients.Group(email).SendAsync("ReceiveNotification", new
            {
                Title = title,
                TitleAr = titleAr,
                Message = message,
                MessageAr = messageAr,
                Type = NotificationType.OrderUpdate.ToString(),
                RelatedId = orderId
            });
        }

        public async Task SendNewProductNotification(string productName)
        {
            var title = "New Item Added!";
            var titleAr = "صنف جديد!";
            var message = $"Check out our new {productName} deliciousness!";
            var messageAr = $"لقد أضفنا {productName} الجديد، جربه الآن!";

            await SaveNotificationAsync(title, titleAr, message, messageAr, NotificationType.NewProduct, "All");

            await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
            {
                Title = title,
                TitleAr = titleAr,
                Message = message,
                MessageAr = messageAr,
                Type = NotificationType.NewProduct.ToString()
            });
        }

        public async Task NotifyAdminNewOrder(string orderId, decimal total)
        {
            var title = "New Order!";
            var message = $"Order #{orderId} received. Total: {total:c}";

            await SaveNotificationAsync(title, "طلب جديد!", message, $"تم استلام الطلب رقم #{orderId}. الإجمالي: {total:c}", NotificationType.NewOrder, "Admins", orderId);

            await _hubContext.Clients.Group("Admins").SendAsync("ReceiveAdminNotification", new
            {
                Title = title,
                Message = message,
                Type = NotificationType.NewOrder.ToString(),
                RelatedId = orderId
            });
        }

        public async Task NotifyAdminNewReview(string productName, string userName, int rating)
        {
            var title = "New Review";
            var message = $"{userName} rated {productName} with {rating} stars.";

            await SaveNotificationAsync(title, "تقييم جديد", message, $"قام {userName} بتقييم {productName} بـ {rating} نجوم.", NotificationType.NewReview, "Admins");

            await _hubContext.Clients.Group("Admins").SendAsync("ReceiveAdminNotification", new
            {
                Title = title,
                Message = message,
                Type = NotificationType.NewReview.ToString()
            });
        }
    }
}
