namespace MangaRestaurant.SaasControl.Entities
{
    /// <summary>
    /// Represents a restaurant (tenant) in the SaaS platform.
    /// Each tenant has its own separate Store and Identity databases.
    /// </summary>
    public class Tenant
    {
        public int Id { get; set; }

        /// <summary>Restaurant name in English</summary>
        public string Name { get; set; }

        /// <summary>Restaurant name in Arabic</summary>
        public string NameAr { get; set; }

        /// <summary>URL-safe unique identifier (e.g., "kfc-egypt")</summary>
        public string Slug { get; set; }

        /// <summary>Name of the Store database for this tenant (e.g., "Restaurant_kfc_egypt")</summary>
        public string StoreDbName { get; set; }

        /// <summary>Name of the Identity database for this tenant (e.g., "Identity_kfc_egypt")</summary>
        public string IdentityDbName { get; set; }

        /// <summary>Email of the first admin who created this restaurant</summary>
        public string AdminEmail { get; set; }

        /// <summary>Optional custom domain (e.g., "www.myrestaurant.com")</summary>
        public string? CustomDomain { get; set; }

        /// <summary>Logo URL for the restaurant</summary>
        public string? LogoUrl { get; set; }

        /// <summary>Whether this tenant is currently active</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>When this tenant was created</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ── Subscription ──
        public string PlanId { get; set; } = "free";
        public Plan Plan { get; set; }

        public DateTime? SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }

        /// <summary>Notes or reason for suspension</summary>
        public string? SuspensionReason { get; set; }
    }
}
