using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.Specifications;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaRestaurant.Repository
{
    internal static class SpecificationsEvaluator<TEntity>where TEntity : BaseEntity
    {
        public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery, ISpecifications<TEntity> spec)
        {
            var query = inputQuery;

            if (spec.Criteria is not null)
                query = query.Where(spec.Criteria);

            //query = _dbContext.Set<Product>().Where(P => P.Id ==1)
            // Includes
            // 1. P => P.Brand
            // 2. P => P.Category
            
            if(spec.OrderBy is not null)
                query = query.OrderBy(spec.OrderBy);

            if(spec.OrderByDescending is not null)  
                query = query.OrderByDescending(spec.OrderByDescending);

            if(spec.IsPaginationEnabled)
                query=query.Skip(spec.Skip).Take(spec.Take);

            query = spec.Includes.Aggregate(query, (currentQuery, includeExpression) => currentQuery.Include(includeExpression));
                //query = _dbContext.Set<Product>().Where(P => P.Id ==1).Include(P => P.Brand)
                //query = _dbContext.Set<Product>().Where(P => P.Id ==1).Include(P => P.Brand).Include(P => P.Category)

                //query = _dbContext.Set<Product>().Where(spec.Criteria);

                return query;
        }
    }
}
