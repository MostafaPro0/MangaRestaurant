using MangaRestaurant.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaRestaurant.Repository.Identity
{
    public static class AppIdentityDbContextSeed
    {
        public static async Task SeedUserAsync(UserManager<AppUser> userManager)
        {
           if(!userManager.Users.Any())
            {
                var user = new AppUser()
                {
                    DisplayName="Mostafa Mohamed",
                    Email="MostafaPro0@yahoo.com",
                    PhoneNumber="01008161832",
                    UserName="MostafaPro"
                };

                await userManager.CreateAsync(user,"Mostafa123@"); 
            }
        }
    }
}
