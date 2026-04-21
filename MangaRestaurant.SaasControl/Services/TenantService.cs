using MangaRestaurant.SaasControl.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace MangaRestaurant.SaasControl.Services
{
    /// <summary>
    /// Resolves the current tenant from the HTTP request using:
    /// 1. Subdomain (production): "kfc.yoursaas.com" → slug = "kfc"
    /// 2. X-Tenant-Slug header (development)
    /// 3. ?tenant=kfc query string (testing)
    /// 
    /// Caches tenant info for 5 minutes to avoid hitting SaasControlDB on every request.
    /// </summary>
    public class TenantService : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SaasControlContext _saasDb;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;

        public TenantService(
            IHttpContextAccessor httpContextAccessor,
            SaasControlContext saasDb,
            IMemoryCache cache,
            IConfiguration config)
        {
            _httpContextAccessor = httpContextAccessor;
            _saasDb = saasDb;
            _cache = cache;
            _config = config;
        }

        public string? GetCurrentTenantSlug()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            // Check if already resolved by middleware
            if (context.Items.TryGetValue("TenantSlug", out var slug) && slug is string s)
                return s;

            return ResolveTenantSlug(context);
        }

        public async Task<TenantInfo?> GetCurrentTenantAsync()
        {
            var slug = GetCurrentTenantSlug();
            if (string.IsNullOrEmpty(slug)) return null;

            return await GetTenantBySlugAsync(slug);
        }

        public TenantInfo? GetCurrentTenantInfoSync()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null && context.Items.TryGetValue("TenantInfo", out var tenant) && tenant is TenantInfo t)
            {
                return t;
            }
            return null;
        }

        public async Task<TenantInfo?> GetTenantBySlugAsync(string slug)
        {
            var cacheKey = $"tenant_{slug}";

            if (_cache.TryGetValue(cacheKey, out TenantInfo? cached))
                return cached;

            var tenant = await _saasDb.Tenants
                .Include(t => t.Plan)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Slug == slug && t.IsActive);

            if (tenant == null) return null;

            var sqlServer = _config["SqlServer:Host"] ?? ".\\MSSQLSERVER2022";

            var tenantInfo = new TenantInfo
            {
                Id = tenant.Id,
                Slug = tenant.Slug,
                Name = tenant.Name,
                StoreConnectionString = BuildConnectionString(sqlServer, tenant.StoreDbName),
                IdentityConnectionString = BuildConnectionString(sqlServer, tenant.IdentityDbName),
                PlanId = tenant.PlanId,
                IsActive = tenant.IsActive,
                // Plan features
                HasLuckyRewards = tenant.Plan?.HasLuckyRewards ?? false,
                HasAdvancedReports = tenant.Plan?.HasAdvancedReports ?? false,
                HasCustomDomain = tenant.Plan?.HasCustomDomain ?? false,
                HasDeliveryTracking = tenant.Plan?.HasDeliveryTracking ?? false,
                MaxProducts = tenant.Plan?.MaxProducts ?? 20,
                MaxStaff = tenant.Plan?.MaxStaff ?? 2
            };

            // Cache for 5 minutes
            _cache.Set(cacheKey, tenantInfo, TimeSpan.FromMinutes(5));

            return tenantInfo;
        }

        private string BuildConnectionString(string server, string database)
        {
            return $"Server={server};Database={database};Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True";
        }

        private string? ResolveTenantSlug(HttpContext context)
        {
            // Priority 1: Subdomain (production)
            var host = context.Request.Host.Host;
            var parts = host.Split('.');
            if (parts.Length >= 3 && parts[0] != "www" && parts[0] != "api")
                return parts[0];

            // Priority 2: X-Tenant-Slug header (development)
            if (context.Request.Headers.TryGetValue("X-Tenant-Slug", out var headerSlug))
                return headerSlug.ToString();

            // Priority 3: Query string (testing only)
            if (context.Request.Query.TryGetValue("tenant", out var querySlug))
                return querySlug.ToString();

            return null;
        }
    }
}
