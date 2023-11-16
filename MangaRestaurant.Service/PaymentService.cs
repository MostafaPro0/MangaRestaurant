using MangaRestaurant.Core;
using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.Entities.Order;
using MangaRestaurant.Core.RepositoriesContract;
using MangaRestaurant.Core.Service;
using Microsoft.Extensions.Configuration;
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

        public PaymentService(IConfiguration configuration, IBasketRepository basketRepository, IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _basketRepository = basketRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<CustomerBasket?> CreateOrUpdatePaymentIntent(string basketId)
        {
            StripeConfiguration.ApiKey = _configuration["StripeKeys:SecretKey"];
            var basket = await _basketRepository.GetBasketAsync(basketId);
            if (basket is null) return null;
            var shippingCost = 0M;
            if (basket.DeliveryMethodId.HasValue)
            {
                var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetAsync(basket.DeliveryMethodId.Value);
                shippingCost = deliveryMethod.Cost;
            }
            if (basket.Items.Count > 0)
            {
                foreach (var item in basket.Items)
                {
                    var product = await _unitOfWork.Repository<Product>().GetAsync(item.Id);
                    if (product is null) continue;
                    if (product is not null)
                    {
                        if (item.Price != product.Price)
                            item.Price = product.Price;
                    }
                }
            }
            var subTotal = basket.Items.Sum(x => x.Price * x.Quantity);

            PaymentIntent paymentIntent;
            var payemntService = new PaymentIntentService();
            if (string.IsNullOrEmpty(basket.PaymentIntentId))
            {
                //create
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
                //Update
                var paymentOptions = new PaymentIntentUpdateOptions()
                {
                    Amount = ((long)subTotal * 100) + ((long)shippingCost * 100)
                };
                paymentIntent = await payemntService.UpdateAsync(basket.PaymentIntentId, paymentOptions);
                basket.PaymentIntentId = paymentIntent.Id;
                basket.ClientSecret = paymentIntent.ClientSecret;
            }
            await _basketRepository.UpdateBasketAsync(basket);
            return basket;
        }
    }
}
