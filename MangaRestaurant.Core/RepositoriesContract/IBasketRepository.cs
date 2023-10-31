using MangaRestaurant.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaRestaurant.Core.RepositoriesContract
{
    public interface IBasketRepository
    {
        Task<CustomerBasket> GetBasketAsync(int id);

        Task<CustomerBasket?> GetBasketAsync(string BasketId);

        Task<CustomerBasket?> UpdateBasketAsync(CustomerBasket basket);
        Task<bool> DeleteBasketAsync(int id);


    }
}
