using System.Text.Json.Serialization;

namespace MangaRestaurant.SaasControl.Entities
{
    /// <summary>
    /// Subscription plan that determines feature availability for a tenant.
    /// </summary>
    public class Plan
    {
        /// <summary>Plan identifier (1: Free, 2: Pro, 3: Enterprise)</summary>
        public int Id { get; set; }

        /// <summary>Display name in English</summary>
        public string Name { get; set; }

        /// <summary>Display name in Arabic</summary>
        public string NameAr { get; set; }

        /// <summary>Monthly subscription price</summary>
        public decimal MonthlyPrice { get; set; }

        /// <summary>Maximum number of products allowed</summary>
        public int MaxProducts { get; set; }

        /// <summary>Maximum number of staff/employees</summary>
        public int MaxStaff { get; set; }

        /// <summary>Whether Lucky Rewards system is available</summary>
        public bool HasLuckyRewards { get; set; }

        /// <summary>Whether advanced analytics/reports are available</summary>
        public bool HasAdvancedReports { get; set; }

        /// <summary>Whether custom domain mapping is allowed</summary>
        public bool HasCustomDomain { get; set; }

        /// <summary>Whether delivery agent tracking is available</summary>
        public bool HasDeliveryTracking { get; set; }

        /// <summary>Whether email notifications are enabled</summary>
        public bool HasEmailNotifications { get; set; }

        /// <summary>Sort order for displaying plans</summary>
        public int SortOrder { get; set; }

        // Navigation
        [JsonIgnore]
        public ICollection<Tenant> Tenants { get; set; } = new HashSet<Tenant>();
    }
}
