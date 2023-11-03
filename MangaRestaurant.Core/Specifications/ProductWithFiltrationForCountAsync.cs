using MangaRestaurant.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaRestaurant.Core.Specifications
{
    public class ProductWithFiltrationForCountAsync:BaseSpecifications<Product>
    {


        public ProductWithFiltrationForCountAsync(ProductSpecParams specParams)
            : base(P =>
                    (string.IsNullOrEmpty(specParams.Search) || P.Name.ToLower().Contains(specParams.Search)) &&
                    (!specParams.BrandId.HasValue || P.BrandId == specParams.BrandId)
                    &&
                    (!specParams.CategoryId.HasValue || P.CategoryId == specParams.CategoryId))
        {

        }
    }
}
