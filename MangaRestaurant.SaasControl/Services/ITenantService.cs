namespace MangaRestaurant.SaasControl.Services
{
    /// <summary>
    /// Resolved tenant information for the current request.
    /// </summary>
    public class TenantInfo
    {
        public int Id { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
        public string StoreConnectionString { get; set; }
        public string IdentityConnectionString { get; set; }
        public string PlanId { get; set; }
        public bool IsActive { get; set; }

        // Plan feature flags (cached from Plan entity)
        public bool HasLuckyRewards { get; set; }
        public bool HasAdvancedReports { get; set; }
        public bool HasCustomDomain { get; set; }
        public bool HasDeliveryTracking { get; set; }
        public int MaxProducts { get; set; }
        public int MaxStaff { get; set; }
    }

    /// <summary>
    /// Service to resolve the current tenant from the HTTP request.
    /// Injected as Scoped — one instance per request.
    /// </summary>
    public interface ITenantService
    {
        string? GetCurrentTenantSlug();
        Task<TenantInfo?> GetTenantBySlugAsync(string slug);
        Task<TenantInfo?> GetCurrentTenantAsync();
        TenantInfo? GetCurrentTenantInfoSync();
    }
}
