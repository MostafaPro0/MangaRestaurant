using MangaRestaurant.SaasControl.Services;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MangaRestaurant.APIs.Middlewares
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
        {
            // Skip tenant resolution for Super Admin endpoints
            if (context.Request.Path.StartsWithSegments("/api/super-admin"))
            {
                await _next(context);
                return;
            }

            var slug = tenantService.GetCurrentTenantSlug();

            if (string.IsNullOrEmpty(slug))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Tenant not identified. Please use a valid tenant subdomain or provide the X-Tenant-Slug header.");
                return;
            }

            // Fetch full tenant info asynchronously
            var tenantInfo = await tenantService.GetTenantBySlugAsync(slug);

            if (tenantInfo == null)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync($"Tenant '{slug}' not found or suspended.");
                return;
            }

            // Store it in the request context for downstream components
            context.Items["TenantSlug"] = slug;
            context.Items["TenantInfo"] = tenantInfo;

            await _next(context);
        }
    }
}
