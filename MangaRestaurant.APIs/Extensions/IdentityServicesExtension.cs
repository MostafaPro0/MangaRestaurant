using MangaRestaurant.Core.Entities.Identity;
using MangaRestaurant.Repository.Identity;
using Microsoft.AspNetCore.Identity;

namespace MangaRestaurant.APIs.Extensions
{
    public static class IdentityServicesExtension
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services)
        {
            services.AddIdentity<AppUser, IdentityRole>(Oprions =>
            {
                //   Oprions.Password.RequiredLength = 5;

            }).AddEntityFrameworkStores<AppIdentityDbContext>();
            services.AddAuthentication();
            return services;
        }
    }
}
