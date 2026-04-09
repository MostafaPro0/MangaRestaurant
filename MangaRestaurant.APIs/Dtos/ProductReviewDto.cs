using System;

namespace MangaRestaurant.APIs.Dtos
{
    public class ProductReviewDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ProductReviewCreateDto
    {
        public int ProductId { get; set; }
        public int Rating { get; set; } 
        public string Comment { get; set; }
    }
}
