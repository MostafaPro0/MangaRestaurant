using System.Threading.Tasks;

namespace MangaRestaurant.Core.Service
{
    public interface INotificationService
    {
        Task SendOrderStatusUpdate(string email, string orderId, string status);
        Task SendNewProductNotification(string productName);
        Task NotifyAdminNewOrder(string orderId, decimal total);
        Task NotifyAdminNewReview(string productName, string userName, int rating);
        Task SendPriceUpdatedNotification(int productId, string productName, string productNameAr, decimal newPrice);
        Task SendSettingsUpdatedNotification();
    }
}
