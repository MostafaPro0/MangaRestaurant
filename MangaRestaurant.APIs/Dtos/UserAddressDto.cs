using MangaRestaurant.Core.Entities.Identity;
using System.ComponentModel.DataAnnotations;

namespace MangaRestaurant.APIs.Dtos
{
    public class UserAddressDto
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Street { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Country { get; set; }
    }
}
