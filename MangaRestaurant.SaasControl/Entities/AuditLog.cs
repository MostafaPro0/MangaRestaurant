namespace MangaRestaurant.SaasControl.Entities
{
    /// <summary>
    /// Audit log for tracking important SaaS-level events.
    /// </summary>
    public class AuditLog
    {
        public int Id { get; set; }

        /// <summary>Which tenant this event relates to (null for global events)</summary>
        public int? TenantId { get; set; }
        public Tenant? Tenant { get; set; }

        /// <summary>Type of event (e.g., "TenantCreated", "PlanChanged", "TenantSuspended")</summary>
        public string EventType { get; set; }

        /// <summary>Detailed description of the event</summary>
        public string Description { get; set; }

        /// <summary>Who performed the action (email or system)</summary>
        public string PerformedBy { get; set; }

        /// <summary>When the event occurred</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
