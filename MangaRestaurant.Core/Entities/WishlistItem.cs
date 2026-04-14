using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaRestaurant.Core.Entities
{
    public class WishlistItem : BaseEntity
    {
        public string UserEmail { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
