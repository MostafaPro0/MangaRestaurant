using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaRestaurant.Core.Entities.Order
{
    public class Order : BaseEntity
    {
        public Order()
        {

        }
        public Order(string buyerEmail, OrderAddress shippingAddress, DeliveryMethod? deliveryMethod, ICollection<OrderItem> items, decimal subTotal, decimal discount, string paymentIntentId, OrderType orderType = OrderType.Delivery)
        {
            BuyerEmail = buyerEmail;
            ShippingAddress = shippingAddress;
            DeliveryMethod = deliveryMethod;
            Items = items;
            SubTotal = subTotal;
            Discount = discount;
            PaymentIntentId = paymentIntentId;
            OrderType = orderType;
        }

        public string BuyerEmail { get; set; }
        public DateTimeOffset OrderDate { get; set; } = DateTimeOffset.Now;
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
        public OrderType OrderType { get; set; } = OrderType.Delivery;
        public OrderAddress ShippingAddress { get; set; }
        public DeliveryMethod? DeliveryMethod { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new HashSet<OrderItem>();

        public decimal SubTotal { get; set; }

        public decimal Discount { get; set; }

        public decimal GetTotal() => (SubTotal - Discount) + (DeliveryMethod?.Cost ?? 0);

        public string? PaymentIntentId { get; set; }

        public string? CashierId { get; set; }
        public string? CashierName { get; set; }

        public string? DeliveryPersonId { get; set; }
        public string? DeliveryPersonName { get; set; }

        public string? WaiterId { get; set; }
        public string? WaiterName { get; set; }
    }
}
