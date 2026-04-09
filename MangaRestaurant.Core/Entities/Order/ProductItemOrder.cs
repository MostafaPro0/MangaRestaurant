using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaRestaurant.Core.Entities.Order
{
    public class ProductItemOrder
    {
        public ProductItemOrder()
        {

        }
        public ProductItemOrder(int productId, string productName, string productNameAr, string pictureUrl)
        {
            ProductId = productId;
            ProductName = productName;
            ProductNameAr = productNameAr;
            PictureUrl = pictureUrl;
        }

        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductNameAr { get; set; }

        public string PictureUrl { get; set; }
    }
}
