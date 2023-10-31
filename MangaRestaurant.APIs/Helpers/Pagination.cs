using MangaRestaurant.Core.Dtos;
using MangaRestaurant.Core.Specifications;

namespace MangaRestaurant.APIs.Helpers
{
    public class Pagination<T>
    {
        private ProductSpecParams specParams;
        private IReadOnlyList<ProductToReturnDto> mappedProducts;

        public Pagination(int pageIndex, int pageSize, ProductSpecParams specParams, IReadOnlyList<T> data,int? count) 
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            Data = data;
            Count = count;
        }

        public int PageSize { get; set; }

        public int PageIndex { get; set; }

        public int? Count { get; set; }

        public IReadOnlyList<T> Data { get; set; }
    }
}
