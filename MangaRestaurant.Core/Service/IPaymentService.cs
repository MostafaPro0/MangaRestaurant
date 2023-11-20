using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.Entities.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaRestaurant.Core.Service
{
    public interface IPaymentService
    {
        Task<CustomerBasket?> CreateOrUpdatePaymentIntent(string basketId);
    
        Task<Order> UpdatePaymentIntentToSuccessOrFailed(string PaymentIntentId, bool flag);
    }
}
