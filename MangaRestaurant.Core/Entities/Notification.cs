using System;

namespace MangaRestaurant.Core.Entities
{
    public enum NotificationType
    {
        General,
        OrderUpdate,
        NewProduct,
        PasswordChange,
        NewReview,
        NewOrder
    }

    public class Notification : BaseEntity
    {
        public string Title { get; set; }
        public string TitleAr { get; set; }
        public string Message { get; set; }
        public string MessageAr { get; set; }
        public NotificationType Type { get; set; }
        
        /// <summary>
        /// Optional ID to link the notification to a specific item (e.g., OrderId, ProductId)
        /// </summary>
        public string RelatedId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// Can be a specific user's email, "All" for all users, or "Admins" for admins only.
        /// </summary>
        public string TargetUser { get; set; } 
    }
}
