using MangaRestaurant.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MangaRestaurant.Repository.Identity
{
    public static class AppIdentityDbContextSeed
    {
        /// <summary>
        /// Full Seed for the Main Platform (Manga Restaurant)
        /// Includes SuperAdmin, Admins, and Staff.
        /// </summary>
        public static async Task SeedUserAsync(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // 1. Ensure all Roles exist
            foreach (var roleName in Enum.GetNames(typeof(AppUserRoles)))
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            // 2. Seed default Super Admin (Mostafa)
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
                    await userManager.AddToRoleAsync(adminUser, AppUserRoles.SuperAdmin.ToString());
                    await userManager.AddToRoleAsync(adminUser, AppUserRoles.Admin.ToString());
                }
            }

            // 3. Seed Platform Staff Members
            var staffMembers = new List<(AppUser user, AppUserRoles role, string password)>
            {
                (new AppUser { DisplayName = "أحمد محمد", Email = "ahmed_cashier@manga.com", UserName = "ahmed_cashier" }, AppUserRoles.Admin, "Ahmed123@"),
                (new AppUser { DisplayName = "محمد صبحي", Email = "moh_delivery@manga.com", UserName = "moh_delivery" }, AppUserRoles.Delivery, "Mohamed123@"),
                (new AppUser { DisplayName = "محمود حسن", Email = "mah_delivery@manga.com", UserName = "mah_delivery" }, AppUserRoles.Delivery, "Mahmoud123@")
            };

            foreach (var staff in staffMembers)
            {
                if (!userManager.Users.Any(u => u.Email == staff.user.Email))
                {
                    var result = await userManager.CreateAsync(staff.user, staff.password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(staff.user, staff.role.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Clean Seed for a NEW Tenant (Customer Restaurant)
        /// Includes only necessary roles and the specific Tenant Admin.
        /// </summary>
        public static async Task SeedNewTenantAsync(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, string adminName, string adminEmail, string adminPassword)
        {
            // 1. Create essential roles only (Exclude SuperAdmin for tenants)
            foreach (var roleName in Enum.GetNames(typeof(AppUserRoles)))
            {
                if (roleName == AppUserRoles.SuperAdmin.ToString()) continue;

                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            // 2. Add the specific admin for this tenant
            if (!userManager.Users.Any(u => u.Email == adminEmail))
            {
                var adminUser = new AppUser 
                { 
                    DisplayName = adminName, 
                    UserName = adminEmail, 
                    Email = adminEmail,
                    PhoneNumber = "0112345678"
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, AppUserRoles.Admin.ToString());
                }
            }
        }
    }
}
