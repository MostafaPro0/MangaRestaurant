using AutoMapper;
using MangaRestaurant.Core.Dtos;
using MangaRestaurant.Core.Entities;

namespace MangaRestaurant.APIs.Helpers
{
    public class ProductPictureUrlResolver : IValueResolver<Product, ProductToReturnDto, string>
    {
        private readonly IConfiguration _configuration;

        public ProductPictureUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string Resolve(Product source, ProductToReturnDto destination, string destMember, ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(source.PictureUrl))
                return $"{_configuration["BaseURL"]}/{source.PictureUrl}";

            return String.Empty ;   
        }
    }
}
