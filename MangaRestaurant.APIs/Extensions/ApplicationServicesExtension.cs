using MangaRestaurant.APIs.Errors;
using MangaRestaurant.APIs.Helpers;
using MangaRestaurant.Core;
using MangaRestaurant.Core.RepositoriesContract;
using MangaRestaurant.Core.Service;
using MangaRestaurant.Repository;
using MangaRestaurant.Service;
using Microsoft.AspNetCore.Mvc;

namespace MangaRestaurant.APIs.Extensions
{
    public static class ApplicationServicesExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services) {

            //كان ممكن نعمل كلاس كلاس 
            //services.AddScoped<IGenericRepository<Product>, GenericRepository<Product>>();
            //services.AddScoped<IGenericRepository<ProductBrand>, GenericRepository<ProductBrand>>();
            //services.AddScoped < IGenericRepository<ProductCategory>, GenericRepository < ProductCategory>>();

            //ده بيعوض اللى فات كله
         //   services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
         //حذفتها علشان استخدمنا unitofwork
         //خلاص مش محتاجينها
            services.AddScoped(typeof(IBasketRepository), typeof(BasketRepository));//AllowDependencyInjection

            services.AddScoped<IUnitOfWork, UnitOfWork>();//AllowDependencyInjection

            services.AddScoped<IOrderService, OrderService>();//AllowDependencyInjection

            services.AddAutoMapper(typeof(MappingProfile));

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = (actionContext) =>
                {
                    var errors = actionContext.ModelState.Where(P => P.Value.Errors.Count() > 0)
                                                         .SelectMany(P => P.Value.Errors)
                                                         .Select(E => E.ErrorMessage)
                                                         .ToArray();

                    var validationErrorRespone = new ApiValidationErrorResponse()
                    {
                        Errors = errors
                    };

                    return new BadRequestObjectResult(validationErrorRespone);

                };

            });

            return services;
        }
    }
}
