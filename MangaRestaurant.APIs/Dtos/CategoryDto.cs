namespace MangaRestaurant.APIs.Dtos
{
    public class CategoryDto
    {
        public string Name { get; set; }
        public string NameAr { get; set; }
        public bool IsHidden { get; set; } = false;
    }
}
