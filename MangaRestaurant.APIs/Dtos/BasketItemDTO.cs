using System.ComponentModel.DataAnnotations;

namespace MangaRestaurant.APIs.Dtos
{
    public class BasketItemDTO
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string PrictureUrl { get; set; }
        [Required]
        [Range(0, double.MaxValue, ErrorMessage="Price Can not Be Zero")]
        public decimal Price { get; set; }
        [Required]
        public string Brand { get; set; }
        [Required]
        public string Category { get; set; }
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Quantity Must Be More Than Zero")]
        public decimal Quantity { get; set; }
    }
}