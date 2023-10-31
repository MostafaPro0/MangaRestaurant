using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaRestaurant.Core.Entities
{
    public class ProductBrand : BaseEntity
    {
        public string Name { get; set; }
       // public ICollection<Product> Products { get; set; } = new HashSet<Product>();
    }
}
