using AutoMapper;
using MangaRestaurant.APIs.Dtos;
using MangaRestaurant.APIs.Errors;
using MangaRestaurant.Core;
using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Localization;
using MangaRestaurant.APIs.Resources;

namespace MangaRestaurant.APIs.Controllers
{
    public class ReviewsController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public ReviewsController(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService, IStringLocalizer<SharedResource> localizer)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
            _localizer = localizer;
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ProductReviewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductReviewDto>> CreateReview(ProductReviewCreateDto reviewDto)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var name = User.FindFirstValue(ClaimTypes.GivenName) ?? User.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(email)) 
                return BadRequest(new ApiResponse(400, _localizer["USER_EMAIL_NOT_FOUND"]));

            // Optional: Check if product exists
            var product = await _unitOfWork.Repository<Product>().GetAsync(reviewDto.ProductId);
            if (product == null) return NotFound(new ApiResponse(404, _localizer["PRODUCT_NOT_FOUND"]));

            var review = _mapper.Map<ProductReviewCreateDto, ProductReview>(reviewDto);
            review.Email = email;
            review.UserName = name ?? email.Split('@')[0];

            await _unitOfWork.Repository<ProductReview>().AddAsync(review);
            var result = await _unitOfWork.CompleteAsync();

            if (result <= 0) return BadRequest(new ApiResponse(400, _localizer["REVIEW_ADD_FAILED"]));

            // Notify Admin
            await _notificationService.NotifyAdminNewReview(product.Name, review.UserName, review.Rating);

            return Ok(_mapper.Map<ProductReview, ProductReviewDto>(review));
        }

        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IReadOnlyList<ProductReviewDto>>> GetReviewsForProduct(int productId)
        {
            // Simple generic repository usage. For more complex filtration, use Specs.
            var reviews = await _unitOfWork.Repository<ProductReview>().GetAllAsync();
            var filteredReviews = reviews.Where(r => r.ProductId == productId).OrderByDescending(r => r.CreatedAt).ToList();

            return Ok(_mapper.Map<IReadOnlyList<ProductReview>, IReadOnlyList<ProductReviewDto>>(filteredReviews));
        }
    }
}
