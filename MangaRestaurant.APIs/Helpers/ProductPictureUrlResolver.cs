using AutoMapper;
using MangaRestaurant.APIs.Dtos;
using MangaRestaurant.Core.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace MangaRestaurant.APIs.Helpers
{
    public class ProductPictureUrlResolver : IValueResolver<Product, ProductToReturnDto, string>
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private const string DefaultImage = "Images/Products/PSX_20221227_162135.jpg";

        public ProductPictureUrlResolver(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        public string Resolve(Product source, ProductToReturnDto destination, string destMember, ResolutionContext context)
        {
            var baseUrl = _configuration["BaseURL"];

            if (!string.IsNullOrEmpty(source.PictureUrl))
            {
                // Check if the physical file exists on disk
                var physicalPath = Path.Combine(_hostingEnvironment.WebRootPath, source.PictureUrl);
                
                if (File.Exists(physicalPath))
                {
                    return $"{baseUrl}/{source.PictureUrl}";
                }
            }

            // Fallback to the legendary Mango/Manga default image if not found or empty
            return $"{baseUrl}/{DefaultImage}";
        }
    }
}
