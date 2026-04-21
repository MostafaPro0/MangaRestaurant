using MangaRestaurant.APIs.Dtos.SuperAdmin;
using MangaRestaurant.APIs.Errors;
using MangaRestaurant.APIs.Services;
using MangaRestaurant.SaasControl.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MangaRestaurant.APIs.Controllers.SuperAdmin
{
    [Authorize(Roles = "SuperAdmin")]
    [Route("api/super-admin/tenants")]
    public class TenantsController : BaseApiController
    {
        private readonly SaasControlContext _saasDb;
        private readonly TenantOnboardingService _onboardingService;

        public TenantsController(SaasControlContext saasDb, TenantOnboardingService onboardingService)
        {
            _saasDb = saasDb;
            _onboardingService = onboardingService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllTenants()
        {
            var tenants = await _saasDb.Tenants.Include(t => t.Plan).ToListAsync();
            return Ok(tenants);
        }

        [HttpGet("{slug}")]
        public async Task<ActionResult> GetTenant(string slug)
        {
            var tenant = await _saasDb.Tenants.Include(t => t.Plan).FirstOrDefaultAsync(t => t.Slug == slug);
            if (tenant == null) return NotFound(new ApiResponse(404));

            return Ok(tenant);
        }

        [HttpPost]
        public async Task<ActionResult> CreateTenant([FromBody] CreateTenantDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(400, "Validation Failed"));

            var result = await _onboardingService.CreateNewTenantAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(new ApiResponse(400, result.ErrorMessage));

            return Ok(result.Tenant);
        }

        [HttpDelete("{slug}")]
        public async Task<ActionResult> DeleteTenant(string slug)
        {
            // WARNING: In a real system, you might not drop the DB directly here, 
            // but just soft delete the Tenant record. Soft delete is safer.
            
            var tenant = await _saasDb.Tenants.FirstOrDefaultAsync(t => t.Slug == slug);
            if (tenant == null) return NotFound(new ApiResponse(404));

            tenant.IsActive = false;
            tenant.SuspensionReason = "Deleted by SuperAdmin";

            _saasDb.AuditLogs.Add(new MangaRestaurant.SaasControl.Entities.AuditLog
            {
                EventType = "TenantSoftDeleted",
                Description = $"Tenant {slug} suspended and marked as deleted",
                PerformedBy = User.Identity?.Name ?? "System"
            });

            await _saasDb.SaveChangesAsync();

            return Ok(new { message = "Tenant successfully deactivated" });
        }
    }
}
