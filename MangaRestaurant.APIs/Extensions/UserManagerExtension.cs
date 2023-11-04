using MangaRestaurant.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MangaRestaurant.APIs.Extensions
{
    public static class UserManagerExtension
    {
        public static async Task<AppUser?> FindUserWithAddressAsync( this UserManager<AppUser> userManager, ClaimsPrincipal _user)
        {
            var email = _user.FindFirstValue(ClaimTypes.Email);
            var user = await userManager.Users.Include(U => U.UserAddress).FirstOrDefaultAsync(U => U.Email == email);
            return user;
        }
    }
}
