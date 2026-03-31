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
        public static async Task SeedUserAsync(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            var adminRoleName = "Admin";
            if (!await roleManager.RoleExistsAsync(adminRoleName))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRoleName));
            }

            if (!userManager.Users.Any(u => u.Email == "MostafaPro0@yahoo.com"))
            {
                var adminUser = new AppUser()
                {
                    DisplayName = "Mostafa Mohamed",
                    Email = "MostafaPro0@yahoo.com",
                    PhoneNumber = "01008161832",
                    UserName = "MostafaPro"
                };

                var createResult = await userManager.CreateAsync(adminUser, "Mostafa123@");
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, adminRoleName);
                }
            }

            // ensure first registered user also is admin for compatibility
            if (!userManager.Users.Any())
            {
                var fallbackAdmin = new AppUser()
                {
                    DisplayName = "Mostafa Mohamed",
                    Email = "MostafaPro0@yahoo.com",
                    PhoneNumber = "01008161832",
                    UserName = "MostafaPro"
                };
                var fallbackResult = await userManager.CreateAsync(fallbackAdmin, "Mostafa123@");
                if (fallbackResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(fallbackAdmin, adminRoleName);
                }
            }
        }
    }
}
