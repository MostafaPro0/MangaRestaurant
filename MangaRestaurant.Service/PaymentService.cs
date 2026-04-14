using MangaRestaurant.Core;
using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.Entities.Order;
using MangaRestaurant.Core.RepositoriesContract;
using MangaRestaurant.Core.Service;
using MangaRestaurant.Core.Specifications.OrderSpecs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Product = MangaRestaurant.Core.Entities.Product;

namespace MangaRestaurant.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly IBasketRepository _basketRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHostEnvironment _environment;

        public PaymentService(IConfiguration configuration, IBasketRepository basketRepository, IUnitOfWork unitOfWork, IHostEnvironment environment)
        {
            _configuration = configuration;
            _basketRepository = basketRepository;
            _unitOfWork = unitOfWork;
            _environment = environment;
        }
        public async Task<CustomerBasket?> CreateOrUpdatePaymentIntent(string basketId)
        {
            var basket = await _basketRepository.GetBasketAsync(basketId);
            if (basket is null) return null;

            var settings = await _unitOfWork.Repository<SiteSettings>().GetAllAsync();
            var shippingCost = settings.FirstOrDefault()?.DeliveryFee ?? 0;

            if (basket.Items.Count > 0)
            {
                foreach (var item in basket.Items)
                {
                    var product = await _unitOfWork.Repository<Product>().GetAsync(item.Id);
                    if (product is null) continue;
                    if (item.Price != product.Price)
                        item.Price = product.Price;
                }
            }

            var subTotal = basket.Items.Sum(x => x.Price * x.Quantity);

            // Bypass Stripe in Development Environment
            if (_environment.IsDevelopment())
            {
                if (string.IsNullOrEmpty(basket.PaymentIntentId))
                {
                    basket.PaymentIntentId = "dev_payment_intent_" + Guid.NewGuid().ToString();
                    basket.ClientSecret = "dev_client_secret_" + Guid.NewGuid().ToString();
                }
            }
            else
            {
                StripeConfiguration.ApiKey = _configuration["StripeKeys:SecretKey"];
                PaymentIntent paymentIntent;
                var payemntService = new PaymentIntentService();

                if (string.IsNullOrEmpty(basket.PaymentIntentId))
                {
                    var paymentOptions = new PaymentIntentCreateOptions()
                    {
                        Amount = ((long)subTotal * 100) + ((long)shippingCost * 100),
                        Currency = "usd",
                        PaymentMethodTypes = new List<string> { "card" }
                    };
                    paymentIntent = await payemntService.CreateAsync(paymentOptions);
                    basket.PaymentIntentId = paymentIntent.Id;
                    basket.ClientSecret = paymentIntent.ClientSecret;
                }
                else
                {
                    var paymentOptions = new PaymentIntentUpdateOptions()
                    {
                        Amount = ((long)subTotal * 100) + ((long)shippingCost * 100)
                    };
                    paymentIntent = await payemntService.UpdateAsync(basket.PaymentIntentId, paymentOptions);
                    basket.PaymentIntentId = paymentIntent.Id;
                    basket.ClientSecret = paymentIntent.ClientSecret;
                }
            }

            await _basketRepository.UpdateBasketAsync(basket);
            return basket;
        }

        public async Task<Order> UpdatePaymentIntentToSuccessOrFailed(string PaymentIntentId, bool flag)
        {
            var spec = new OrderWithPaymentIntentSpecifications(PaymentIntentId);
            var order = await _unitOfWork.Repository<Order>().GetEntityWithSpecAsync(spec);
            if (flag)
            {
                order.OrderStatus = OrderStatus.PaymentReceived;
            }
            else
            {
                order.OrderStatus = OrderStatus.PaymentFailed;
            }
            _unitOfWork.Repository<Order>().Update(order);
            await _unitOfWork.CompleteAsync();
            return order;
        }
    }
}

