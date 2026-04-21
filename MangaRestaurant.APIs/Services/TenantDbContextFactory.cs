using MangaRestaurant.Repository.Data;
using MangaRestaurant.Repository.Identity;
using MangaRestaurant.SaasControl.Services;
using Microsoft.EntityFrameworkCore;

namespace MangaRestaurant.APIs.Services
{
    public class TenantDbContextFactory
    {
        private readonly ITenantService _tenantService;
        private readonly ILoggerFactory _loggerFactory;

        public TenantDbContextFactory(ITenantService tenantService, ILoggerFactory loggerFactory)
        {
            _tenantService = tenantService;
            _loggerFactory = loggerFactory;
        }

        public StoreContext CreateStoreContext()
        {
            var tenant = _tenantService.GetCurrentTenantInfoSync();
            if (tenant == null) throw new UnauthorizedAccessException("Tenant could not be resolved.");

            var optionsBuilder = new DbContextOptionsBuilder<StoreContext>();
            optionsBuilder.UseSqlServer(tenant.StoreConnectionString);
            optionsBuilder.UseLoggerFactory(_loggerFactory);

            return new StoreContext(optionsBuilder.Options);
        }

        public AppIdentityDbContext CreateIdentityContext()
        {
            var tenant = _tenantService.GetCurrentTenantInfoSync();
            if (tenant == null) throw new UnauthorizedAccessException("Tenant could not be resolved.");

            var optionsBuilder = new DbContextOptionsBuilder<AppIdentityDbContext>();
            optionsBuilder.UseSqlServer(tenant.IdentityConnectionString);

            return new AppIdentityDbContext(optionsBuilder.Options);
        }
    }
}
