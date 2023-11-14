using MangaRestaurant.Core;
using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.RepositoriesContract;
using MangaRestaurant.Repository.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaRestaurant.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly StoreContext _dbContext;
        private Hashtable _repositories;

        public UnitOfWork(StoreContext dbContext)
        {
            _dbContext = dbContext;
            _repositories= new Hashtable();
        }
        public async Task<int> CompleteAsync() => await _dbContext.SaveChangesAsync();

        public ValueTask DisposeAsync() => _dbContext.DisposeAsync();

        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
        {
            var typeName=typeof(TEntity).Name;
            if(!_repositories.ContainsKey(typeName))
            {
                var repository = new GenericRepository<TEntity>(_dbContext);
               _repositories.Add(typeName, repository);
            }
            return _repositories[typeName] as IGenericRepository<TEntity>;
        }
    }
}
