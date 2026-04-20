using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaRestaurant.Core.Entities
{
    public class SiteSettings : BaseEntity
    {
        public string RestaurantName { get; set; }
        public string RestaurantNameAr { get; set; }
        public string Address { get; set; }
        public string AddressAr { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Email { get; set; }
        public string CurrencyCode { get; set; } // e.g., USD, EGP
        public string CurrencySymbol { get; set; } // e.g., $, ج.م
        public string FacebookUrl { get; set; }
        public string InstagramUrl { get; set; }
        public string TwitterUrl { get; set; }
        public string OpeningHoursEn { get; set; }
        public string OpeningHoursAr { get; set; }
        public decimal DeliveryFee { get; set; }
        public bool IsLuckyRewardsEnabled { get; set; } = true;
    }
}
