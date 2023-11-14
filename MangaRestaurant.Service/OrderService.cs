using MangaRestaurant.Core;
using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.Entities.Order;
using MangaRestaurant.Core.RepositoriesContract;
using MangaRestaurant.Core.Service;
using MangaRestaurant.Core.Specifications.OrderSpecs;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IBasketRepository basketRepository, IUnitOfWork unitOfWork)
        {
            _basketRepository = basketRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<Order?> CreateOrderAsync(string buyerEmail, string basketId, int methodId, OrderAddress orderShippingAddress)
        {
            // Get Basket From Basket Repo.
            var basket = await _basketRepository.GetBasketAsync(basketId);
            var orderItems = new List<OrderItem>();

            //Get Selected Item At Basket From Product Repo
            if (basket?.Items.Count > 0)
            {
                foreach (var item in basket.Items)
                {
                    var product = await _unitOfWork.Repository<Product>().GetAsync(item.Id);
                    var productItemOrder = new ProductItemOrder(product.Id, product.Name, product.PictureUrl);
                    var orderItem = new OrderItem(productItemOrder, product.Price, item.Quantity);
                    orderItems.Add(orderItem);
                }
            }

            //Get SubTotal
            var subTotal = orderItems.Sum(item => item.Price * item.Quantity);

            //Get Delivery Method from Delivery Methd Repo
            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetAsync(methodId);

            //Discount when add it i will add it (Mostafa)
            decimal Disount = 0;

            var order = new Order(buyerEmail, orderShippingAddress, deliveryMethod, orderItems, subTotal, Disount);

            await _unitOfWork.Repository<Order>().AddAsync(order);

            //Save Order To Database
            var result = await _unitOfWork.CompleteAsync();

            if (result <= 0) return null;
            return order;
        }

        public Task<Order> GetOrderByIdForSpecificUserAsync(string buyerEmail, int orderId)
        {
            var spec = new OrderSpecifications(buyerEmail, orderId);
            var orders = _unitOfWork.Repository<Order>().GetAsyncWithSpecAsync(spec);
            return orders;
        }

        public Task<IReadOnlyList<Order>> GetOrdersForSpecificUserAsync(string buyerEmail)
        {
            var spec = new OrderSpecifications(buyerEmail);
            var orders = _unitOfWork.Repository<Order>().GetAllAsyncWithSpecAsync(spec);
            return orders;
        }

        public async Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodsAsync()
        {
            var deliveryMethods = await _unitOfWork.Repository<DeliveryMethod>().GetAllAsync();
            return deliveryMethods;
        }
    }
}
