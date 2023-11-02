using System.ComponentModel.DataAnnotations;

namespace MangaRestaurant.APIs.Dtos
{
    public class RegisterDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string DisplayName { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        [RegularExpression("^(?=.*[a-zA-Z])(?=.*\\d).{8,}$", ErrorMessage ="the password contains both letters and numbers and has a minimum length of 8 characters. You can modify the minimum length or other requirements to suit your specific needs.")]
        public string Password { get; set; }
    }
}
