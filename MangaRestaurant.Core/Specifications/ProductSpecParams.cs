using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaRestaurant.Core.Specifications
{
    public class ProductSpecParams
    {
        public string? Sort { get; set; }
        public int? BrandId { get; set; }
        public int? CategoryId { get; set; }

        private int pageSize = 20;

        public int PageSize
        {
            get { return pageSize; }
            set { pageSize = pageSize > 20 ? 20 : value; }
        }

        public int PageIndex { get; set; } = 1;
    }
}
