
using MangaRestaurant.APIs.Errors;
using MangaRestaurant.APIs.Extensions;
using MangaRestaurant.APIs.Helpers;
using MangaRestaurant.APIs.Middlewares;
using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.Entities.Identity;
using MangaRestaurant.Core.RepositoriesContract;
using MangaRestaurant.Repository;
using MangaRestaurant.Repository.Data;
using MangaRestaurant.Repository.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Writers;
using StackExchange.Redis;

using MangaRestaurant.APIs.Hubs;

namespace MangaRestaurant.APIs
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSwaggerServices();
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

            #region Configuration Service
            builder.Services.AddControllers();
            builder.Services.AddSignalR(); 

            builder.Services.AddApplicationServices();

            builder.Services.AddDbContext<MangaRestaurant.SaasControl.Data.SaasControlContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("SaasControlConnection"));
            });

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<MangaRestaurant.SaasControl.Services.ITenantService, MangaRestaurant.SaasControl.Services.TenantService>();
            builder.Services.AddScoped<MangaRestaurant.APIs.Services.TenantDbContextFactory>();
            builder.Services.AddScoped<MangaRestaurant.APIs.Services.TenantOnboardingService>();

            builder.Services.AddScoped<StoreContext>(provider => {
                var factory = provider.GetRequiredService<MangaRestaurant.APIs.Services.TenantDbContextFactory>();
                return factory.CreateStoreContext();
            });

            builder.Services.AddSingleton<IConnectionMultiplexer>(Option =>
            {
                var connection = (builder.Configuration.GetConnectionString("RedisConnection"));
                return ConnectionMultiplexer.Connect(connection);
            });

            builder.Services.AddScoped<AppIdentityDbContext>(provider => {
                var factory = provider.GetRequiredService<MangaRestaurant.APIs.Services.TenantDbContextFactory>();
                return factory.CreateIdentityContext();
            });

            builder.Services.AddIdentityServices(builder.Configuration);
            builder.Services.AddCors(Options =>
            {
                Options.AddPolicy("MyPolicy", options =>
                {
                    options.AllowAnyHeader();
                    options.AllowAnyMethod();
                    options.AllowCredentials();
                    options.WithOrigins(builder.Configuration["FrontBaseURL"] ?? "http://localhost:4200");
                });
            });
            #endregion

            var app = builder.Build();

            #region UpdateDatabase
            using var scope = app.Services.CreateScope();

            var services = scope.ServiceProvider;

            var loggerFacotory = services.GetRequiredService<ILoggerFactory>();
            try
            {
                var saasDb = services.GetRequiredService<MangaRestaurant.SaasControl.Data.SaasControlContext>();
                await saasDb.Database.MigrateAsync();

                // Call the separate Seed class
                await MangaRestaurant.SaasControl.Data.SaasControlContextSeed.SeedAsync(saasDb, loggerFacotory);

                // Note: StoreContext and AppIdentityDbContext are now tenant-specific
                // Migrations for them will be handled later via an admin endpoint.
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                var logger = loggerFacotory.CreateLogger<Program>();
                logger.LogError(ex, "an error has been occured during apply the migration");
            }
            #endregion
            #region Configure Kestrel Middlewares
            app.UseMiddleware<ExceptionMiddleware>();

            var supportedCultures = new[] { "ar", "en" };
            var localizationOptions = new RequestLocalizationOptions()
                .SetDefaultCulture(supportedCultures[0])
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);

            app.UseRequestLocalization(localizationOptions);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                //  app.UseDeveloperExceptionPage();    
                app.UseSwaggerMiddlewares();
            }

            app.UseStatusCodePagesWithReExecute("/errors/{0}");
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCors("MyPolicy");
            app.UseMiddleware<MangaRestaurant.APIs.Middlewares.TenantMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<NotificationHub>("/hub/notifications");
            app.MapHub<DeliveryHub>("/hub/delivery");

            #endregion

            app.Run();
        }
    }
}