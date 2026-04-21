using MangaRestaurant.SaasControl.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MangaRestaurant.SaasControl.Data.Configurations
{
    public class PlanConfiguration : IEntityTypeConfiguration<Plan>
    {
        public void Configure(EntityTypeBuilder<Plan> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasMaxLength(50);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.NameAr)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.MonthlyPrice)
                .HasColumnType("decimal(18,2)");

            // Seed default plans
            builder.HasData(
                new Plan
                {
                    Id = "free",
                    Name = "Free",
                    NameAr = "مجاني",
                    MonthlyPrice = 0,
                    MaxProducts = 20,
                    MaxStaff = 2,
                    HasLuckyRewards = false,
                    HasAdvancedReports = false,
                    HasCustomDomain = false,
                    HasDeliveryTracking = false,
                    HasEmailNotifications = false,
                    SortOrder = 1
                },
                new Plan
                {
                    Id = "pro",
                    Name = "Professional",
                    NameAr = "احترافي",
                    MonthlyPrice = 99,
                    MaxProducts = 200,
                    MaxStaff = 10,
                    HasLuckyRewards = true,
                    HasAdvancedReports = true,
                    HasCustomDomain = false,
                    HasDeliveryTracking = true,
                    HasEmailNotifications = true,
                    SortOrder = 2
                },
                new Plan
                {
                    Id = "enterprise",
                    Name = "Enterprise",
                    NameAr = "مؤسسي",
                    MonthlyPrice = 299,
                    MaxProducts = int.MaxValue,
                    MaxStaff = int.MaxValue,
                    HasLuckyRewards = true,
                    HasAdvancedReports = true,
                    HasCustomDomain = true,
                    HasDeliveryTracking = true,
                    HasEmailNotifications = true,
                    SortOrder = 3
                }
            );
        }
    }
}
