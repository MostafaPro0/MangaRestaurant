using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaRestaurant.Core.Entities.Order
{
    public class DeliveryMethod:BaseEntity
    {
        public DeliveryMethod() 
        {
        
        } 
        public DeliveryMethod(int deliveryMethodId, string shortName, string description, string deliveryTime, decimal cost)
        {
            DeliveryMethodId = deliveryMethodId;
            ShortName = shortName;
            Description = description;
            DeliveryTime = deliveryTime;
            Cost = cost;
        }

        public int DeliveryMethodId { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public string DeliveryTime { get; set; }
        public decimal Cost { get; set; }
    }
}
