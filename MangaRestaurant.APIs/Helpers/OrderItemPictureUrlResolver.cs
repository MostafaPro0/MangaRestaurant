using AutoMapper;
using MangaRestaurant.APIs.Dtos;
using MangaRestaurant.Core.Entities.Order;

namespace MangaRestaurant.APIs.Helpers
{
    public class OrderItemPictureUrlResolver : IValueResolver<OrderItem, OrderItemDTO, string>
    {
        private readonly IConfiguration _configuration;

        public OrderItemPictureUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string Resolve(OrderItem source, OrderItemDTO destination, string destMember, ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(source.ProductItemOrder.PictureUrl))
                return $"{_configuration["BaseURL"]}/{source.ProductItemOrder.PictureUrl}";

            return String.Empty;
        }
    }
}
