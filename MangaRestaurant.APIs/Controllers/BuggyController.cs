using MangaRestaurant.APIs.Errors;
using MangaRestaurant.Repository.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MangaRestaurant.APIs.Controllers
{
    public class BuggyController : BaseApiController
    {
        private readonly StoreContext _dbContext;

        public BuggyController(StoreContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET : api/buggy/notfound
        [HttpGet("notfound")]
        public ActionResult GetNotFoundRequest()
        {
            var product = _dbContext.Products.Find(100);

            if (product is null)
                return NotFound(new ApiResponse(404));

            return Ok(product);
        }

        // GET : api/buggy/servererror
        [HttpGet("servererror")]
        public ActionResult GetServerError()
        {
            var product = _dbContext.Products.Find(100);
            var productToReturn = product.ToString();

            return Ok(productToReturn);
        }

        // GET : api/buggy/badrequest
        [HttpGet("badrequest")]
        public ActionResult GetBadRequest()
        {
            return BadRequest(new ApiResponse(400));
        }

        // GET : api/buggy/badrequest/five 
        [HttpGet("badrequest/{Id}")]
        public ActionResult GetBadRequest(int id) //Validation Error
        {
            return Ok(new ApiResponse(400));
        }

        // GET : api/buggy/unauthorized 
        [HttpGet("unauthorized")]
        public ActionResult GetUnauthorizedError()
        {
            return Unauthorized(new ApiResponse(401));
        }

    }
}
