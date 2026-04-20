using AutoMapper;
using MangaRestaurant.APIs.Dtos;
using MangaRestaurant.APIs.Errors;
using MangaRestaurant.Core;
using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MangaRestaurant.Repository.Data;

namespace MangaRestaurant.APIs.Controllers
{
    public class LuckyRewardsController : BaseApiController
    {
        private readonly StoreContext _context;
        private readonly UserManager<AppUser> _userManager;

        public LuckyRewardsController(StoreContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // --- PUBLIC ENDPOINTS ---

        [HttpGet("status")]
        public async Task<ActionResult<object>> GetLuckyStatus()
        {
            var settings = await _context.SiteSettings.OrderByDescending(s => s.Id).FirstOrDefaultAsync();
            if (settings == null) return Ok(new { isEnabled = false });
            return Ok(new { isEnabled = settings.IsLuckyRewardsEnabled });
        }

        [Authorize]
        [HttpGet("coins")]
        public async Task<ActionResult<int>> GetUserCoins()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return NotFound(new ApiResponse(404));

            return Ok(user.LuckyCoins);
        }

        [Authorize]
        [HttpPost("draw")]
        public async Task<ActionResult<object>> DrawReward()
        {
            var settings = await _context.SiteSettings.OrderByDescending(s => s.Id).FirstOrDefaultAsync();
            if (settings == null || !settings.IsLuckyRewardsEnabled) 
                return BadRequest(new ApiResponse(400, "Lucky Rewards system is currently disabled."));

            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return NotFound(new ApiResponse(404));

            if (user.LuckyCoins < 1)
                return BadRequest(new ApiResponse(400, "Not enough coins."));

            var prizes = await _context.LuckyPrizes.Where(p => p.IsActive).ToListAsync();
            if (!prizes.Any()) return BadRequest(new ApiResponse(400, "No prizes available."));

            // Calculate random prize based on probability
            int totalWeight = prizes.Sum(p => p.ProbabilityWeight);
            int randomNum = new Random().Next(0, totalWeight);
            int currentSum = 0;
            LuckyPrize wonPrize = prizes.Last();

            foreach (var prize in prizes)
            {
                currentSum += prize.ProbabilityWeight;
                if (randomNum < currentSum)
                {
                    wonPrize = prize;
                    break;
                }
            }

            // Deduct coin and save reward
            user.LuckyCoins -= 1;
            await _userManager.UpdateAsync(user);

            var reward = new UserLuckyReward
            {
                AppUserId = user.Id,
                LuckyPrizeId = wonPrize.Id,
                WonAt = DateTime.UtcNow,
                PromoCode = "REWARD-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper()
            };
            
            _context.UserLuckyRewards.Add(reward);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                PrizeId = wonPrize.Id,
                Title = wonPrize.Title,
                TitleAr = wonPrize.TitleAr,
                Description = wonPrize.Description,
                DescriptionAr = wonPrize.DescriptionAr,
                Icon = wonPrize.Icon,
                Color = wonPrize.Color,
                RemainingCoins = user.LuckyCoins
            });
        }

        // --- ADMIN ENDPOINTS ---

        [Authorize(Roles = "Admin")]
        [HttpGet("prizes")]
        public async Task<ActionResult<List<LuckyPrize>>> GetPrizes()
        {
            var prizes = await _context.LuckyPrizes.ToListAsync();
            return Ok(prizes);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("prizes")]
        public async Task<ActionResult<LuckyPrize>> AddPrize(LuckyPrize prize)
        {
            _context.LuckyPrizes.Add(prize);
            await _context.SaveChangesAsync();
            return Ok(prize);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("prizes")]
        public async Task<ActionResult<LuckyPrize>> UpdatePrize(LuckyPrize prize)
        {
            _context.LuckyPrizes.Update(prize);
            await _context.SaveChangesAsync();
            return Ok(prize);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("prizes/{id}")]
        public async Task<ActionResult> DeletePrize(int id)
        {
            var prize = await _context.LuckyPrizes.FindAsync(id);
            if (prize == null) return NotFound(new ApiResponse(404));
            
            _context.LuckyPrizes.Remove(prize);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
