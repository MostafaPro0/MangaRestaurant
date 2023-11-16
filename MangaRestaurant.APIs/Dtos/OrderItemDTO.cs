using MangaRestaurant.Core.Entities.Order;

namespace MangaRestaurant.APIs.Dtos
{
    public class OrderItemDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string PictureUrl { get; set; }
        public decimal Price { get; set; }

        public decimal Quantity { get; set; }
    }
}