using MangaRestaurant.Core.Entities;
using System.Collections.Generic;

namespace MangaRestaurant.Core.Specifications
{
    public class NotificationsForUserSpecification : BaseSpecifications<Notification>
    {
        public NotificationsForUserSpecification(string email, bool isAdmin) 
            : base(n => n.TargetUser == email || n.TargetUser == "All" || (isAdmin && n.TargetUser == "Admins"))
        {
            AddOrderByDescending(n => n.CreatedAt);
        }
    }
}
