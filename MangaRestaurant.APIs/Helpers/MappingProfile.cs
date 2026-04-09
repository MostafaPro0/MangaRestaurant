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
                .ForMember(d => d.BrandAr, O => O.MapFrom(s => s.Brand.NameAr))
                .ForMember(d => d.Category, O => O.MapFrom(s => s.Category.Name))
                .ForMember(d => d.CategoryAr, O => O.MapFrom(s => s.Category.NameAr))
                .ForMember(d => d.PictureUrl, O => O.MapFrom<ProductPictureUrlResolver>())
                .ForMember(d => d.AverageRating, O => O.MapFrom(s => s.Reviews.Any() ? Math.Round(s.Reviews.Average(r => r.Rating), 1) : 0));

            CreateMap<ProductCreateDto, Product>();
            CreateMap<ProductReview, ProductReviewDto>();
            CreateMap<ProductReviewCreateDto, ProductReview>();

            CreateMap<UserAddress, UserAddressDto>().ReverseMap();
            CreateMap<UserAddressDto, OrderAddress>();

            CreateMap<CustomerBasketDTO, CustomerBasket>().ReverseMap();
            CreateMap<BasketItemDTO, BasketItem>().ReverseMap();
            CreateMap<Order, OrderToReturnDTO>()
                .ForMember(D => D.DeliveryMethod, O => O.MapFrom(S => S.DeliveryMethod.ShortName))
                .ForMember(D => D.DeliveryMethodCost, O => O.MapFrom(S => S.DeliveryMethod.Cost))
                .ForMember(D => D.DeliveryPersonName, O => O.MapFrom(S => S.DeliveryPersonName))
                .ForMember(D => D.WaiterName, O => O.MapFrom(S => S.WaiterName))
                .ForMember(D => D.CashierName, O => O.MapFrom(S => S.CashierName));
            CreateMap<OrderItem, OrderItemDTO>()
                .ForMember(OI => OI.ProductId, O => O.MapFrom(S => S.ProductItemOrder.ProductId))
                .ForMember(OI => OI.ProductName, O => O.MapFrom(S => S.ProductItemOrder.ProductName))
                .ForMember(OI => OI.ProductNameAr, O => O.MapFrom(S => S.ProductItemOrder.ProductNameAr))
                .ForMember(OI => OI.PictureUrl, O => O.MapFrom<OrderItemPictureUrlResolver>())
                .ForMember(OI => OI.CurrentPictureUrl, O => O.Ignore());

            CreateMap<AppUser, UserDTO>()
                .ForMember(d => d.ProfilePictureUrl, o => o.MapFrom<ProfilePictureUrlResolver>());
                
            CreateMap<Notification, NotificationToReturnDto>()
                .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()));
                
            CreateMap<SiteSettings, SiteSettingsDto>().ReverseMap();
        }
    }
}