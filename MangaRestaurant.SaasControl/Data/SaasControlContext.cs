using MangaRestaurant.SaasControl.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace MangaRestaurant.SaasControl.Data
{
    /// <summary>
    /// Database context for the SaaS Control plane.
    /// This is a CENTRAL database (one per entire platform) that stores:
    /// - Tenants (restaurants)
    /// - Subscription Plans
    /// - Audit Logs
    /// 
    /// Each tenant's actual data (products, orders, etc.) lives in a SEPARATE database.
    /// </summary>
    public class SaasControlContext : DbContext
    {
        public SaasControlContext(DbContextOptions<SaasControlContext> options) : base(options)
        {
        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Apply all configurations from this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
    }
}
