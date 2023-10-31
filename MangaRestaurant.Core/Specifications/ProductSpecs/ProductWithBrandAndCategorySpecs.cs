using MangaRestaurant.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaRestaurant.Core.Specifications.ProductSpecs
{
    public class ProductWithBrandAndCategorySpecs : BaseSpecifications<Product>
    {
        // This Constructor will be Used for Creating an Object, That will be Used to Get All Products 
        public ProductWithBrandAndCategorySpecs(ProductSpecParams specParams)
        :base(P =>
                    (!specParams.BrandId.HasValue || P.BrandId == specParams.BrandId)
                    &&
                    (!specParams.CategoryId.HasValue || P.CategoryId== specParams.CategoryId))
        {
            AddIncludes();
            AddSort(specParams.Sort);
            AddPagination(specParams.PageSize*(specParams.PageIndex-1), specParams.PageSize);
        }
        // This Constructor will be Used for Creating an Object, That will be Used to Get Specific Products 
        public ProductWithBrandAndCategorySpecs(int id)
        : base(P => P.Id==id)
        {
            AddIncludes();
        }
        public void AddSort(string? Sort)
        {
            if (!string.IsNullOrEmpty(Sort))
            {
                switch (Sort)
                {
                    case "PriceAsc":
                        AddOrderBy(P => P.Price);
                        break;
                    case "PriceDesc":
                        AddOrderByDescending(P => P.Price);
                        break;
                    default:
                        AddOrderBy(P => P.Name);
                        break;
                }
            }
        }
        private void AddIncludes()
        {

            Includes.Add(P => P.Brand);
            Includes.Add(P => P.Category);

        }


    }

    
}
