using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaRestaurant.Core.Entities.Order
{
    public class OrderItem:BaseEntity
    {
        public OrderItem()
        {

        }
        public OrderItem(ProductItemOrder productItemOrder, decimal price, decimal quantity)
        {
            ProductItemOrder = productItemOrder;
            Price = price;
            Quantity = quantity;
        }

        public ProductItemOrder ProductItemOrder { get; set; }
        public decimal Price { get; set; }

        public decimal Quantity { get; set; }
    }
}
