using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using MangaRestaurant.APIs.Dtos;
using MangaRestaurant.APIs.Errors;
using MangaRestaurant.Core;
using MangaRestaurant.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace MangaRestaurant.APIs.Controllers
{
    [Authorize]
    public class WishlistController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public WishlistController(IUnitOfWork unitOfWork, IMapper mapper, IStringLocalizer<SharedResource> localizer)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizer = localizer;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ProductToReturnDto>>> GetUserWishlist()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var wishlists = await _unitOfWork.Repository<WishlistItem>().GetAllAsync();
            
            var productIds = wishlists
                .Where(w => w.UserEmail == email)
                .Select(w => w.ProductId)
                .ToList();

            var products = await _unitOfWork.Repository<Product>().GetAllAsync();
            var userProducts = products.Where(p => productIds.Contains(p.Id)).ToList();

            return Ok(_mapper.Map<IReadOnlyList<Product>, IReadOnlyList<ProductToReturnDto>>(userProducts));
        }

        [HttpPost("{productId}")]
        public async Task<ActionResult<ApiResponse>> ToggleWishlist(int productId)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var product = await _unitOfWork.Repository<Product>().GetAsync(productId);
            
            if (product == null) return NotFound(new ApiResponse(404));

            var wishlists = await _unitOfWork.Repository<WishlistItem>().GetAllAsync();
            var existingItem = wishlists.FirstOrDefault(w => w.UserEmail == email && w.ProductId == productId);

            if (existingItem != null)
            {
                _unitOfWork.Repository<WishlistItem>().Delete(existingItem);
                await _unitOfWork.CompleteAsync();
                return Ok(new ApiResponse(200, "Removed from wishlist"));
            }

            var newItem = new WishlistItem
            {
                UserEmail = email,
                ProductId = productId
            };

            await _unitOfWork.Repository<WishlistItem>().AddAsync(newItem);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, "Added to wishlist"));
        }

        [HttpDelete("{productId}")]
        public async Task<ActionResult<ApiResponse>> RemoveFromWishlist(int productId)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var wishlists = await _unitOfWork.Repository<WishlistItem>().GetAllAsync();
            var item = wishlists.FirstOrDefault(w => w.UserEmail == email && w.ProductId == productId);

            if (item == null) return NotFound(new ApiResponse(404));

            _unitOfWork.Repository<WishlistItem>().Delete(item);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, "Removed from wishlist"));
        }

        [HttpGet("check/{productId}")]
        public async Task<ActionResult<bool>> IsInWishlist(int productId)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var wishlists = await _unitOfWork.Repository<WishlistItem>().GetAllAsync();
            var exists = wishlists.Any(w => w.UserEmail == email && w.ProductId == productId);
            return Ok(exists);
        }
    }
}
