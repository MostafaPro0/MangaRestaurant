using MangaRestaurant.Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MangaRestaurant.APIs.Controllers
{
    public class BasketsController : BaseApiController
    {
        // GET Or ReCreate Basket
        [HttpGet("{Id}")]
        public async Task<ActionResult<CustomerBasket>> GetCustomerBasket()
        {
            
        } 
    }
}
