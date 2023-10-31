
using MangaRestaurant.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaRestaurant.Core.Specifications.EmployeeSpecs
{
    public class ProductCategorySpecs : BaseSpecifications<ProductCategory>
    {
        public ProductCategorySpecs(string Sort)
        : base()
        {
            AddSort(Sort);
        }
        public ProductCategorySpecs(int id)
      : base(E=>E.Id == id)
        {

        }
        public void AddSort(string? Sort)
        {
            if (!string.IsNullOrEmpty(Sort))
            {
                switch (Sort)
                {
                    default:
                        AddOrderBy(P => P.Name);
                        break;
                }
            }
        }
    }
}
