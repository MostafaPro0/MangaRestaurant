using MangaRestaurant.Core.Entities.Order;
using System.ComponentModel.DataAnnotations;

namespace MangaRestaurant.APIs.Dtos
{
    public class OrderDTO
    {
        [Required]
        public string BasketId { get; set; }
        public int DeliveryMethodId { get; set; }

        public UserAddressDto ShippingAddress { get; set; }
    }
}
