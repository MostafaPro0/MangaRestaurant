using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaRestaurant.Core.Entities.Identity
{
    public class AppUser:IdentityUser
    {
        public string DisplayName { get; set; } 
        public string? ProfilePictureUrl { get; set; }
        public string? PhoneNumber2 { get; set; }

        public bool IsBanned { get; set; } = false;
        public ICollection<UserAddress> UserAddresses { get; set; } = new HashSet<UserAddress>();
        //Navigational Proporety
    }
}
