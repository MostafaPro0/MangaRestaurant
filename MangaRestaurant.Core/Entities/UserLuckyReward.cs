using System;
using System.ComponentModel.DataAnnotations.Schema;
using MangaRestaurant.Core.Entities.Identity;

namespace MangaRestaurant.Core.Entities
{
    public class UserLuckyReward : BaseEntity
    {
        public string AppUserId { get; set; }

        public int LuckyPrizeId { get; set; }
        
        [ForeignKey("LuckyPrizeId")]
        public LuckyPrize LuckyPrize { get; set; }

        public DateTime WonAt { get; set; } = DateTime.UtcNow;
        public bool IsRedeemed { get; set; } = false;
        public string PromoCode { get; set; } // Generated logic if it's a discount
    }
}
