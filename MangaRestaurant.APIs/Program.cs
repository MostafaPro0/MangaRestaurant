
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

namespace MangaRestaurant.APIs
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSwaggerServices();

            #region Configuration Service
            builder.Services.AddControllers();

            builder.Services.AddDbContext<StoreContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddSingleton<IConnectionMultiplexer>(Option =>
            {
                var connection = (builder.Configuration.GetConnectionString("RedisConnection"));
                return ConnectionMultiplexer.Connect(connection);
            });

            builder.Services.AddDbContext<AppIdentityDbContext>(Options =>
            {
                Options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection"));
            });

            builder.Services.AddApplicationServices();

            builder.Services.AddIdentityServices(builder.Configuration);

            #endregion

            var app = builder.Build();

            #region UpdateDatabase
            using var scope = app.Services.CreateScope();

            var services = scope.ServiceProvider;

            var loggerFacotory = services.GetRequiredService<ILoggerFactory>();
            try
            {
                var _dbContext = services.GetRequiredService<StoreContext>();
                //ASK CLR Creating Object from DbContext Explicitly
                await _dbContext.Database.MigrateAsync();

                var identityDbContext = services.GetRequiredService<AppIdentityDbContext>();    
                await identityDbContext.Database.MigrateAsync();

                await StoreContextSeed.SeedAsync(_dbContext);

                var userManager = services.GetRequiredService<UserManager<AppUser>>();
                await AppIdentityDbContextSeed.SeedUserAsync(userManager);
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

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                //  app.UseDeveloperExceptionPage();    
                app.UseSwaggerMiddlewares();
            }

            app.UseStatusCodePagesWithReExecute("/errors/{0}");

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.MapControllers();

            app.UseAuthorization();
            app.UseAuthentication();
            #endregion

            app.Run();
        }
    }
}