using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.Entities.Order;
using MangaRestaurant.Core.RepositoriesContract;
using MangaRestaurant.Core.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaRestaurant.Service
{
    public class OrderService : IOrderService
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IGenericRepository<Product> _productRepository;
        private readonly IGenericRepository<DeliveryMethod> _deliveryMethodRepository;
        private readonly IGenericRepository<Order> _orderRepository;

        public OrderService(IBasketRepository basketRepository, IGenericRepository<Product> productRepository, IGenericRepository<DeliveryMethod> DeliveryMethodRepository,IGenericRepository<Order> orderRepository)
        {
            _basketRepository = basketRepository;
            _productRepository = productRepository;
            _deliveryMethodRepository = DeliveryMethodRepository;
            _orderRepository = orderRepository;
        }
        public async Task<Order> CreateOrderAsync(string buyerEmail, string basketId, int methodId, OrderAddress orderShippingAddress)
        {
            // Get Basket From Basket Repo.
            var basket = await _basketRepository.GetBasketAsync(basketId);
            var orderItems = new List<OrderItem>();

            //Get Selected Item At Basket From Product Repo
            if (basket?.Items.Count > 0)
            {
                foreach (var item in basket.Items)
                {
                    var product = await _productRepository.GetAsync(item.Id);
                    var productItemOrder = new ProductItemOrder(product.Id, product.Name, product.PictureUrl);
                    var orderItem = new OrderItem(productItemOrder, product.Price, item.Quantity);
                    orderItems.Add(orderItem);
                }
            }

            //Get SubTotal
            var subTotal = orderItems.Sum(item => item.Price * item.Quantity);

            //Get Delivery Method from Delivery Methd Repo
            var deliveryMethod = await _deliveryMethodRepository.GetAsync(methodId);

            //Discount when add it i will add it (Mostafa)
            decimal Disount = 0;

            var order = new Order(buyerEmail, orderShippingAddress, deliveryMethod, orderItems, subTotal, Disount);

          await   _orderRepository.AddAsync(order);

            return order;
        }

        public Task<Order> GetOrderByIdForSpecificUserAsync(string buyerEmail)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Order>> GetOrdersForSpecificUserAsync(string buyerEmail)
        {
            throw new NotImplementedException();
        }
    }
}
