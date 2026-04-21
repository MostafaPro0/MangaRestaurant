using MangaRestaurant.SaasControl.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MangaRestaurant.SaasControl.Data.Configurations
{
    public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(t => t.NameAr)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(t => t.Slug)
                .IsRequired()
                .HasMaxLength(100);

            // Slug must be unique across all tenants
            builder.HasIndex(t => t.Slug)
                .IsUnique();

            builder.Property(t => t.StoreDbName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(t => t.IdentityDbName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(t => t.AdminEmail)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(t => t.CustomDomain)
                .HasMaxLength(500);

            builder.Property(t => t.LogoUrl)
                .HasMaxLength(1000);

            builder.Property(t => t.PlanId)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("free");

            builder.Property(t => t.SuspensionReason)
                .HasMaxLength(500);

            // Relationship: Tenant → Plan
            builder.HasOne(t => t.Plan)
                .WithMany(p => p.Tenants)
                .HasForeignKey(t => t.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            // Index for custom domain lookup
            builder.HasIndex(t => t.CustomDomain)
                .IsUnique()
                .HasFilter("[CustomDomain] IS NOT NULL");
        }
    }
}
