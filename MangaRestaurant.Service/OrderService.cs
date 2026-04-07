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
        private readonly IPaymentService _paymentService;
        private readonly INotificationService _notificationService;

        public OrderService(IBasketRepository basketRepository, IUnitOfWork unitOfWork, IPaymentService paymentService, INotificationService notificationService)
        {
            _basketRepository = basketRepository;
            _unitOfWork = unitOfWork;
            _paymentService = paymentService;
            _notificationService = notificationService;
        }

        public async Task<Order?> CreateOrderAsync(string buyerEmail, string basketId, int methodId, OrderAddress orderShippingAddress)
        {
            // Get Basket From Basket Repo.
            var basket = await _basketRepository.GetBasketAsync(basketId);
            var orderItems = new List<OrderItem>();

            // Get selected item at basket from product repo
            if (basket?.Items.Count > 0)
            {
                foreach (var item in basket.Items)
                {
                    var product = await _unitOfWork.Repository<Product>().GetAsync(item.Id);
                    if (product == null)
                        continue; // ignore missing products

                    var productItemOrder = new ProductItemOrder(product.Id, product.Name, product.NameAr, product.PictureUrl);
                    var orderItem = new OrderItem(productItemOrder, product.Price, item.Quantity);
                    orderItems.Add(orderItem);
                }
            }

            // Get SubTotal
            var subTotal = orderItems.Sum(item => item.Price * item.Quantity);

            // Get Delivery Method from Delivery Method Repo
            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetAsync(methodId);
            if (deliveryMethod == null)
                return null;

            //Discount when add it i will add it (Mostafa)
            decimal Disount = 0;

            if (basket == null)
                return null;

            // Set basket delivery method so payment intent calculation matches selected delivery method
            if (basket.DeliveryMethodId != methodId)
            {
                basket.DeliveryMethodId = methodId;
                await _basketRepository.UpdateBasketAsync(basket);
            }

            if (orderItems.Count == 0)
                return null;

            // Ensure payment intent exists and is populated in basket
            await _paymentService.CreateOrUpdatePaymentIntent(basketId);
            basket = await _basketRepository.GetBasketAsync(basketId);

            if (basket == null || string.IsNullOrEmpty(basket.PaymentIntentId))
                return null;

            var orderSpec = new OrderWithPaymentIntentSpecifications(basket.PaymentIntentId);
            var exOrder = await _unitOfWork.Repository<Order>().GetEntityWithSpecAsync(orderSpec);
            if (exOrder is not null)
            {
                _unitOfWork.Repository<Order>().Delete(exOrder);
            }

            var order = new Order(buyerEmail, orderShippingAddress, deliveryMethod, orderItems, subTotal, Disount, basket.PaymentIntentId);

            await _unitOfWork.Repository<Order>().AddAsync(order);

            //Save Order To Database
            var result = await _unitOfWork.CompleteAsync();

            if (result <= 0) return null;

            // Notify Admin
            await _notificationService.NotifyAdminNewOrder(order.Id.ToString(), order.GetTotal());

            return order;
        }

        public Task<Order?> GetOrderByIdForSpecificUserAsync(string buyerEmail, int orderId)
        {
            var spec = new OrderSpecifications(buyerEmail, orderId);
            var order = _unitOfWork.Repository<Order>().GetEntityWithSpecAsync(spec);
            return order;
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

        public async Task<IReadOnlyList<Order>> GetAllOrdersAsync()
        {
            var spec = new OrderSpecificationsForAdmin();
            var orders = await _unitOfWork.Repository<Order>().GetAllAsyncWithSpecAsync(spec);
            return orders;
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            var spec = new OrderSpecificationsForAdmin(orderId);
            var order = await _unitOfWork.Repository<Order>().GetEntityWithSpecAsync(spec);
            return order;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _unitOfWork.Repository<Order>().GetAsync(orderId);
            if (order == null) return false;
            
            order.OrderStatus = status;
            _unitOfWork.Repository<Order>().Update(order);
            var result = await _unitOfWork.CompleteAsync();

            if (result > 0)
            {
                await _notificationService.SendOrderStatusUpdate(order.BuyerEmail, order.Id.ToString(), status.ToString());
            }

            return result > 0;
        }
    }
}
