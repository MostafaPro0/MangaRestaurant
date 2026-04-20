using MangaRestaurant.APIs.Errors;
using MangaRestaurant.APIs.Helpers;
using MangaRestaurant.APIs.Services;
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

            // Register generic repository for controllers/services requiring IGenericRepository<T>
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.AddSingleton<IResponseCacheService, ResponseCacheService>();//AllowDependencyInjection

            services.AddScoped(typeof(IBasketRepository), typeof(BasketRepository));//AllowDependencyInjection

            services.AddScoped<IUnitOfWork, UnitOfWork>();//AllowDependencyInjection

            services.AddScoped<IOrderService, OrderService>();//AllowDependencyInjection

            services.AddScoped<IPaymentService, PaymentService>();//AllowDependencyInjection

            services.AddScoped<IEmailService, EmailService>();//AllowDependencyInjection
            services.AddScoped<INotificationService, NotificationService>();
            services.AddSingleton<IEncryptionService, EncryptionService>();


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
