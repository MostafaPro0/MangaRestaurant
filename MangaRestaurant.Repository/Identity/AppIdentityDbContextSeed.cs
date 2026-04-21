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
            var superAdminRoleName = "SuperAdmin";
            var adminRoleName = "Admin";
            var userRoleName = "User";

            var cashierRoleName = "Cashier";
            var deliveryRoleName = "Delivery";
            var waiterRoleName = "Waiter";

            var roles = new List<string> { superAdminRoleName, adminRoleName, userRoleName, cashierRoleName, deliveryRoleName, waiterRoleName };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }


            // Seed default admin user (MostafaPro0@yahoo.com)
            if (!userManager.Users.Any(u => u.Email == "MostafaPro0@yahoo.com"))
            {
                var adminUser = new AppUser()
                {
                    DisplayName = "Mostafa Mohamed",
                    Email = "MostafaPro0@yahoo.com",
                    PhoneNumber = "01008161832",
                    PhoneNumber2 = "01008161833",
                    UserName = "MostafaPro"
                };

                var createResult = await userManager.CreateAsync(adminUser, "Mostafa123@");
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, adminRoleName);
                    await userManager.AddToRoleAsync(adminUser, superAdminRoleName);
                }
            }

            // Seed Staff Members
            var staffMembers = new List<(AppUser user, string role, string password)>
            {
                (new AppUser { DisplayName = "أحمد محمد", Email = "ahmed_cashier@manga.com", UserName = "ahmed_cashier" }, cashierRoleName, "Ahmed123@"),
                (new AppUser { DisplayName = "محمد صبحي", Email = "moh_delivery@manga.com", UserName = "moh_delivery" }, deliveryRoleName, "Mohamed123@"),
                (new AppUser { DisplayName = "محمود حسن", Email = "mah_delivery@manga.com", UserName = "mah_delivery" }, deliveryRoleName, "Mahmoud123@"),
                (new AppUser { DisplayName = "علي إبراهيم", Email = "ali_waiter@manga.com", UserName = "ali_waiter" }, waiterRoleName, "Ali123@")
            };

            foreach (var staff in staffMembers)
            {
                if (!userManager.Users.Any(u => u.Email == staff.user.Email))
                {
                    var result = await userManager.CreateAsync(staff.user, staff.password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(staff.user, staff.role);
                    }
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
                    await userManager.AddToRoleAsync(fallbackAdmin, superAdminRoleName);
                }
            }
        }
    }
}

