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

        public SaaSInfoController(SaasControlContext saasDb)
        {
            _saasDb = saasDb;
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
                    LogoUrl = "assets/images/logo.png" 
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
