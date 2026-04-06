using AutoMapper;
using MangaRestaurant.APIs.Dtos;
using MangaRestaurant.Core.Entities.Identity;
using Microsoft.Extensions.Configuration;

namespace MangaRestaurant.APIs.Helpers
{
    public class ProfilePictureUrlResolver : IValueResolver<AppUser, UserDTO, string?>
    {
        private readonly IConfiguration _configuration;

        public ProfilePictureUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string? Resolve(AppUser source, UserDTO destination, string? destMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(source.ProfilePictureUrl))
            {
                return null;
            }

            // If it's already a full URL (like Google profile pic), return it as is
            if (source.ProfilePictureUrl.StartsWith("http"))
            {
                return source.ProfilePictureUrl;
            }

            // Return the full URL with the BaseURL from settings
            return _configuration["BaseURL"] + source.ProfilePictureUrl;
        }
    }
}
