using MangaRestaurant.Core.Entities.Order;

namespace MangaRestaurant.APIs.Dtos
{
    public class OrderToReturnDTO
    {
        public int Id { get; set; }
        public string BuyerEmail { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public string OrderStatus { get; set; }
        public OrderAddress ShippingAddress { get; set; }
        public string DeliveryMethod { get; set; }
        public decimal DeliveryFee { get; set; }
        public string OrderType { get; set; }

        public ICollection<OrderItemDTO> Items { get; set; } = new HashSet<OrderItemDTO>();

        public decimal SubTotal { get; set; }

        public decimal Discount { get; set; }

        public decimal Total { get; set; }

        public string PaymentIntentId { get; set; }
        
        public string? CashierId { get; set; }
        public string? CashierName { get; set; }

        public string? DeliveryPersonId { get; set; }
        public string? DeliveryPersonName { get; set; }

        public string? WaiterId { get; set; }
        public string? WaiterName { get; set; }
    }
}
