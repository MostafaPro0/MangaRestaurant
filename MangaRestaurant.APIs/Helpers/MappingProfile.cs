using AutoMapper;
using MangaRestaurant.APIs.Dtos;
using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.Entities.Identity;

namespace MangaRestaurant.APIs.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductToReturnDto>()
                .ForMember(d => d.Brand, O => O.MapFrom(s => s.Brand.Name))
                .ForMember(d => d.Category, O => O.MapFrom(s => s.Category.Name))
                .ForMember(d => d.PictureUrl, O => O.MapFrom<ProductPictureUrlResolver>());

            CreateMap<UserAddress, UserAddressDto>().ReverseMap();
        }
    }
}
