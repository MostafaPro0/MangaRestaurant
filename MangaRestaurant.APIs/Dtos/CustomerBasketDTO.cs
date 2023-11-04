using MangaRestaurant.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace MangaRestaurant.APIs.Dtos
{
    public class CustomerBasketDTO
    {
        [Required]
        public string Id { get; set; }
        public List<BasketItemDTO> Items { get; set; }
    }
}
