using System.ComponentModel.DataAnnotations;

namespace MangaRestaurant.APIs.Dtos
{
    public class ProductCreateDto
    {
        [Required]
        public string Name { get; set; }

        public string NameAr { get; set; }

        [Required]
        public string Description { get; set; }

        public string DescriptionAr { get; set; }

        public string PictureUrl { get; set; }

        [Required]
        [Range(0.1, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
        public decimal Price { get; set; }

        public decimal? OldPrice { get; set; }

        [Required]
        public int BrandId { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }
}
