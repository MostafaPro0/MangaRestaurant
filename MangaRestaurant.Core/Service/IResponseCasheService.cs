using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaRestaurant.Core.Service
{
    public interface IResponseCacheService
    {
        // Cash Data
        Task CasheResponseAsync(string CasheKey, object Response, TimeSpan ExpireTime);


        // Get Cashed Data
        Task<string?> GetCashedResponse(string CasheKey);
    }
}
