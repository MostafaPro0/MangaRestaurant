using System.ComponentModel.DataAnnotations;

namespace MangaRestaurant.APIs.Dtos.SuperAdmin
{
    public class CreateTenantDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string NameAr { get; set; }

        [Required]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug can only contain lowercase letters, numbers, and hyphens.")]
        public string Slug { get; set; }

        [Required]
        [EmailAddress]
        public string AdminEmail { get; set; }

        [Required]
        public string AdminName { get; set; }

        [Required]
        [MinLength(6)]
        public string AdminPassword { get; set; }

        public int PlanId { get; set; } = 1;
    }
}
