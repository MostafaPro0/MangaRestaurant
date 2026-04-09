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

        public async Task<Order?> CreateOrderAsync(string buyerEmail, string basketId, OrderAddress orderShippingAddress, OrderType orderType = OrderType.Delivery)
        {
            // Get Basket From Basket Repo.
            var basket = await _basketRepository.GetBasketAsync(basketId);
            if (basket == null) return null;

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

            if (orderItems.Count == 0) return null;

            // Get SubTotal
            var subTotal = orderItems.Sum(item => item.Price * item.Quantity);

            // Calculate Delivery Fee from Settings
            decimal deliveryFee = 0;
            if (orderType == OrderType.Delivery)
            {
                var settings = await _unitOfWork.Repository<SiteSettings>().GetAllAsync();
                deliveryFee = settings.FirstOrDefault()?.DeliveryFee ?? 0;
            }

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

            var order = new Order(buyerEmail, orderShippingAddress, deliveryFee, orderItems, subTotal, basket.Discount, basket.PaymentIntentId, orderType);

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
            return await _unitOfWork.Repository<Order>().GetAllAsync();
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

        public async Task<bool> AssignDeliveryPersonAsync(int orderId, string deliveryPersonId, string deliveryPersonName)
        {
            var order = await _unitOfWork.Repository<Order>().GetAsync(orderId);
            if (order == null) return false;

            order.DeliveryPersonId = deliveryPersonId;
            order.DeliveryPersonName = deliveryPersonName;
            _unitOfWork.Repository<Order>().Update(order);
            var result = await _unitOfWork.CompleteAsync();
            return result > 0;
        }

        public async Task<bool> AssignWaiterAsync(int orderId, string waiterId, string waiterName)
        {
            var order = await _unitOfWork.Repository<Order>().GetAsync(orderId);
            if (order == null) return false;

            order.WaiterId = waiterId;
            order.WaiterName = waiterName;
            _unitOfWork.Repository<Order>().Update(order);
            return await _unitOfWork.CompleteAsync() > 0;
        }
    }
}
