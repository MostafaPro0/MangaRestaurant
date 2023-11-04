using AutoMapper;
using MangaRestaurant.APIs.Dtos;
using MangaRestaurant.APIs.Errors;
using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.RepositoriesContract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MangaRestaurant.APIs.Controllers
{
    public class BasketsController : BaseApiController
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IMapper _mapper;

        public BasketsController(IBasketRepository basketRepository, IMapper mapper)
        {
            _basketRepository = basketRepository;
            _mapper = mapper;
        }
        // GET Or ReCreate Basket
        [HttpGet]//GET OR Create
        public async Task<ActionResult<CustomerBasket>> GetCustomerBasket(string id)
        {
            var basket = await _basketRepository.GetBasketAsync(id);
            return basket is null ? new CustomerBasket(id) : basket;
        }
        // Create Or Update
        [HttpPost]
        public async Task<ActionResult<CustomerBasket>> UpdateBasket(CustomerBasketDTO basket)
        {
            var mappedBasket = _mapper.Map<CustomerBasketDTO, CustomerBasket>(basket);
            var createdOrUpdated = await _basketRepository.UpdateBasketAsync(mappedBasket);

            return createdOrUpdated is null ? BadRequest(new ApiResponse(400, "You Have Problem in your basket")) : Ok(createdOrUpdated);
        }
        // Delete Basket
        [HttpDelete]
        public async Task<ActionResult<bool>> DeleteBasket(string id)
        {
            return await _basketRepository.DeleteBasketAsync(id);
        }
    }
}
