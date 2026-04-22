using MangaRestaurant.SaasControl.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MangaRestaurant.SaasControl.Data
{
    public static class SaasControlContextSeed
    {
        public static async Task SeedAsync(SaasControlContext context, ILoggerFactory loggerFactory)
        {
            try
            {
                // 1. Ensure Plans exist (though mostly handled by Config, this is safe)
                if (!await context.Plans.AnyAsync())
                {
                    // Plans are seeded via PlanConfiguration (IEntityTypeConfiguration)
                    // But if for some reason they aren't there, we could add them.
                    // Since PlanConfiguration was already implemented with HasData, 
                    // EF handles this during Migration.
                }

                // 2. Seed default 'manga' tenant for transition
                if (!await context.Tenants.AnyAsync(t => t.Slug == "manga"))
                {
                    var mangaTenant = new Tenant
                    {
                        Name = "Manga Restaurant",
                        NameAr = "مطعم مانجا",
                        Slug = "manga",
                        StoreDbName = "MangaRestaurantDB", // Points to legacy store DB
                        IdentityDbName = "MangaRestaurantIdentityDB", // Points to legacy identity DB
                        AdminEmail = "MostafaPro0@yahoo.com",
                        PlanId = 2,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        SubscriptionStartDate = DateTime.UtcNow,
                        SubscriptionEndDate = DateTime.UtcNow.AddYears(1)
                    };

                    context.Tenants.Add(mangaTenant);

                    // Add some sample partners for the landing page
                    context.Tenants.AddRange(new List<Tenant>
                    {
                        new Tenant { 
                            Name = "Ramen Station", NameAr = "محطة رامن", Slug = "ramen-station", 
                            AdminEmail = "ramen@example.com", PlanId = 2, IsActive = true,
                            LogoUrl = "https://cdn-icons-png.flaticon.com/512/3443/3443338.png", 
                            CreatedAt = DateTime.UtcNow
                        },
                        new Tenant { 
                            Name = "Sushi Master", NameAr = "سوشي ماستر", Slug = "sushi-master", 
                            AdminEmail = "sushi@example.com", PlanId = 3, IsActive = true,
                            LogoUrl = "https://cdn-icons-png.flaticon.com/512/2252/2252438.png",
                            CreatedAt = DateTime.UtcNow
                        },
                        new Tenant { 
                            Name = "Burger House", NameAr = "بيت البرجر", Slug = "burger-house", 
                            AdminEmail = "burger@example.com", PlanId = 1, IsActive = true,
                            LogoUrl = "https://cdn-icons-png.flaticon.com/512/3075/3075977.png",
                            CreatedAt = DateTime.UtcNow
                        }
                    });

                    context.AuditLogs.Add(new AuditLog
                    {
                        EventType = "SystemSeed",
                        Description = "Seeded default Manga tenant and sample partners",
                        PerformedBy = "System",
                        CreatedAt = DateTime.UtcNow
                    });

                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger("SaasControlSeed");
                logger.LogError(ex, "An error occurred while seeding SaaS Control data.");
            }
        }
    }
}
