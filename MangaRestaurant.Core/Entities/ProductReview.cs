using MangaRestaurant.Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaRestaurant.Core.Entities
{
    public class ProductReview : BaseEntity
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
