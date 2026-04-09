namespace MangaRestaurant.APIs.Dtos
{
    public class UserDTO
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string? Role { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PhoneNumber2 { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string Token { get; set; }
        public bool HasPassword { get; set; }
    }
}
