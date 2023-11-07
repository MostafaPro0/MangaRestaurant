using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MangaRestaurant.Core.RepositoriesContract
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<IReadOnlyList<T>> GetAllAsync();

        Task<T?> GetAsync(int id);

        Task<IReadOnlyList<T>> GetAllAsyncWithSpecAsync(ISpecifications<T> spec);
        Task<T?> GetAsyncWithSpecAsync(ISpecifications<T> spec);
        //Task DeleteAsync(int id);

        //Task<IReadOnlyList<T>> GetAsync()
        Task<int?> GetCountAsyncWithSpecAsync(ISpecifications<T> spec);

        Task AddAsync(T entity);
    }
}
