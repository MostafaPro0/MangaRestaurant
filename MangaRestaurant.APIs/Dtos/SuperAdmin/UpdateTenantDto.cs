namespace MangaRestaurant.APIs.Dtos.SuperAdmin
{
    public class UpdateTenantDto
    {
        public string Name { get; set; }
        public string NameAr { get; set; }
        public int PlanId { get; set; }
        public bool IsActive { get; set; }
    }
}
