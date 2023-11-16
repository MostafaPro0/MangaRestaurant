using AutoMapper;
using MangaRestaurant.APIs.Dtos;
using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.Entities.Identity;
using MangaRestaurant.Core.Entities.Order;

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
            CreateMap<UserAddressDto, OrderAddress>();

            CreateMap<CustomerBasketDTO, CustomerBasket>().ReverseMap();
            CreateMap<BasketItemDTO, BasketItem>().ReverseMap();
            CreateMap<Order, OrderToReturnDTO>()
                .ForMember(D => D.DeliveryMethod, O => O.MapFrom(S => S.DeliveryMethod.ShortName))
                .ForMember(D => D.DeliveryMethodCost, O => O.MapFrom(S => S.DeliveryMethod.Cost));
            CreateMap<OrderItem, OrderItemDTO>()
                .ForMember(OI => OI.ProductId, O => O.MapFrom(S => S.ProductItemOrder.ProductId))
                .ForMember(OI => OI.ProductName, O => O.MapFrom(S => S.ProductItemOrder.ProductName))
                .ForMember(OI => OI.PictureUrl, O => O.MapFrom(S => S.ProductItemOrder.PictureUrl))
                .ForMember(OI => OI.PictureUrl, O => O.MapFrom<OrderItemPictureUrlResolver>());
        }
    }
}