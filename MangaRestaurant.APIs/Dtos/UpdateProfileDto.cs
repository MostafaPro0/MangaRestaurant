namespace MangaRestaurant.APIs.Dtos
{
    public class UpdateProfileDto
    {
        public string DisplayName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PhoneNumber2 { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }
}
