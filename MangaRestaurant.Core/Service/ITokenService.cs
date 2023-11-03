using MangaRestaurant.Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaRestaurant.Core.Service
{
    public interface ITokenService
    {
        Task<string> CreateTokenAsync(AppUser user);
    }
}
