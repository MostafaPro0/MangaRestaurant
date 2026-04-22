using MangaRestaurant.SaasControl.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace MangaRestaurant.APIs.Controllers
{
    [Route("api/saas-info")]
    public class SaaSInfoController : BaseApiController
    {
        private readonly SaasControlContext _saasDb;
        private readonly Services.TenantOnboardingService _onboardingService;

        public SaaSInfoController(SaasControlContext saasDb, Services.TenantOnboardingService onboardingService)
        {
            _saasDb = saasDb;
            _onboardingService = onboardingService;
        }

        [HttpPost("register-restaurant")]
        public async Task<ActionResult> RegisterRestaurant([FromBody] Dtos.SuperAdmin.CreateTenantDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new Errors.ApiResponse(400, "Validation Failed"));

            var result = await _onboardingService.CreateNewTenantAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(new Errors.ApiResponse(400, result.ErrorMessage));

            return Ok(new { 
                message = "Restaurant created successfully!",
                subdomain = result.Tenant.Slug,
                loginUrl = $"{result.Tenant.Slug}.localhost" // In production this would be domain
            });
        }

        [HttpGet("active-tenants")]
        public async Task<ActionResult> GetPublicTenants()
        {
            var tenants = await _saasDb.Tenants
                .Where(t => t.IsActive && t.Slug != "manga")
                .Select(t => new {
                    t.Name,
                    t.NameAr,
                    t.Slug,
                    LogoUrl = t.LogoUrl ?? "assets/images/logo.png",
                    t.CustomDomain
                })
                .ToListAsync();

            return Ok(tenants);
        }

        [HttpGet("plans")]
        public async Task<ActionResult> GetActivePlans()
        {
            var plans = await _saasDb.Plans
                .OrderBy(p => p.MonthlyPrice)
                .ToListAsync();
            return Ok(plans);
        }
    }
}
