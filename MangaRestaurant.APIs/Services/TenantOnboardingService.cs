using MangaRestaurant.APIs.Dtos.SuperAdmin;
using MangaRestaurant.Core.Entities.Identity;
using MangaRestaurant.Repository.Data;
using MangaRestaurant.Repository.Identity;
using MangaRestaurant.SaasControl.Data;
using MangaRestaurant.SaasControl.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MangaRestaurant.APIs.Services
{
    public class TenantCreationResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public Tenant? Tenant { get; set; }

        public static TenantCreationResult Success(Tenant tenant) => new() { IsSuccess = true, Tenant = tenant };
        public static TenantCreationResult Fail(string error) => new() { IsSuccess = false, ErrorMessage = error };
    }

    public class TenantOnboardingService
    {
        private readonly SaasControlContext _saasDb;
        private readonly IConfiguration _config;
        private readonly ILoggerFactory _loggerFactory;

        public TenantOnboardingService(
            SaasControlContext saasDb,
            IConfiguration config,
            ILoggerFactory loggerFactory)
        {
            _saasDb = saasDb;
            _config = config;
            _loggerFactory = loggerFactory;
        }

        public async Task<TenantCreationResult> CreateNewTenantAsync(CreateTenantDto dto)
        {
            try
            {
                // 1. Check if slug exists in SaaS Control
                if (await _saasDb.Tenants.AnyAsync(t => t.Slug == dto.Slug))
                    return TenantCreationResult.Fail("Tenant slug is already taken.");

                // 2. Generate database names
                var baseDbName = dto.Slug.Replace("-", "_");
                var storeDbName = $"Restaurant_{baseDbName}_{DateTime.Now:yyyyMMddHHmmss}";
                var identityDbName = $"Identity_{baseDbName}_{DateTime.Now:yyyyMMddHHmmss}";
                
                var sqlServer = _config["SqlServer:Host"] ?? ".\\MSSQLSERVER2022";
                var storeConn = $"Server={sqlServer};Database={storeDbName};Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True";
                var identityConn = $"Server={sqlServer};Database={identityDbName};Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True";

                // 3. Migrate and Seed Store DB
                var storeOptions = new DbContextOptionsBuilder<StoreContext>()
                    .UseLoggerFactory(_loggerFactory)
                    .UseSqlServer(storeConn)
                    .Options;

                await using (var storeCtx = new StoreContext(storeOptions))
                {
                    await storeCtx.Database.MigrateAsync();
                    await StoreContextSeed.SeedAsync(storeCtx); // Insert default products/categories 
                }

                // 4. Migrate Identity DB
                var identityOptions = new DbContextOptionsBuilder<AppIdentityDbContext>()
                    .UseLoggerFactory(_loggerFactory)
                    .UseSqlServer(identityConn)
                    .Options;

                await using (var identityCtx = new AppIdentityDbContext(identityOptions))
                {
                    await identityCtx.Database.MigrateAsync();
                }

                // 5. Seed Identity DB (Roles, Default users, and Tenant Admin)
                await SeedIdentityDbAsync(identityConn, dto);

                // 6. Record Tenant in SaaS Control
                var tenant = new Tenant
                {
                    Name = dto.Name,
                    NameAr = dto.NameAr,
                    Slug = dto.Slug,
                    StoreDbName = storeDbName,
                    IdentityDbName = identityDbName,
                    AdminEmail = dto.AdminEmail,
                    PlanId = string.IsNullOrEmpty(dto.PlanId) ? "free" : dto.PlanId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    SubscriptionStartDate = DateTime.UtcNow,
                    SubscriptionEndDate = DateTime.UtcNow.AddMonths(1) // 1 month free trial
                };

                _saasDb.Tenants.Add(tenant);
                
                _saasDb.AuditLogs.Add(new AuditLog
                {
                    EventType = "TenantCreated",
                    Description = $"Created new tenant: {tenant.Slug}",
                    PerformedBy = "System"
                });

                await _saasDb.SaveChangesAsync();

                return TenantCreationResult.Success(tenant);
            }
            catch (Exception ex)
            {
                var logger = _loggerFactory.CreateLogger<TenantOnboardingService>();
                logger.LogError(ex, "Failed to provision new tenant {Slug}", dto.Slug);
                return TenantCreationResult.Fail($"Internal provisioning error: {ex.Message}");
            }
        }

        private async Task SeedIdentityDbAsync(string connectionString, CreateTenantDto dto)
        {
            // Create a dedicated DI container to resolve Identity managers locally for this DB
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<AppIdentityDbContext>(options => options.UseSqlServer(connectionString));
            
            services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<AppIdentityDbContext>()
                .AddDefaultTokenProviders();

            using var provider = services.BuildServiceProvider();
            var userManager = provider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

            // 1. Seed base roles and generic users
            await AppIdentityDbContextSeed.SeedUserAsync(userManager, roleManager);

            // 2. Add the specific admin for this tenant
            if (!await userManager.Users.AnyAsync(u => u.Email == dto.AdminEmail))
            {
                var adminUser = new AppUser 
                { 
                    DisplayName = dto.AdminName, 
                    UserName = dto.AdminEmail.Split('@')[0], // safe username
                    Email = dto.AdminEmail,
                    PhoneNumber = "0000000000"
                };

                var result = await userManager.CreateAsync(adminUser, dto.AdminPassword);
                if (result.Succeeded)
                {
                    if (!await roleManager.RoleExistsAsync("Admin"))
                        await roleManager.CreateAsync(new IdentityRole("Admin"));
                        
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}
