using AutoMapper;
using MangaRestaurant.APIs.Dtos;
using MangaRestaurant.APIs.Errors;
using MangaRestaurant.Core;
using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.RepositoriesContract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MangaRestaurant.APIs.Controllers
{
    public class BasketController : BaseApiController
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public BasketController(IBasketRepository basketRepository, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _basketRepository = basketRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }
        // GET Or ReCreate Basket
        [HttpGet]//GET OR Create
        public async Task<ActionResult<CustomerBasket>> GetCustomerBasket(string id)
        {
            var basket = await _basketRepository.GetBasketAsync(id);
            if (basket == null) return Ok(new CustomerBasket(id));

            // Sync prices from DB for all items in the basket
            var productRepo = _unitOfWork.Repository<Product>();

            bool changed = false;
            foreach (var item in basket.Items)
            {
                var product = await productRepo.GetAsync(item.Id);
                if (product != null && product.Price != item.Price)
                {
                    item.Price = product.Price;
                    changed = true;
                }
            }

            if (changed)
            {
                await _basketRepository.UpdateBasketAsync(basket);
            }

            return Ok(basket);
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
