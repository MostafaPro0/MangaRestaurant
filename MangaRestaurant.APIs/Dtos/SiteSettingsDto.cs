namespace MangaRestaurant.APIs.Dtos
{
    public class SiteSettingsDto
    {
        public string RestaurantName { get; set; }
        public string RestaurantNameAr { get; set; }
        public string Address { get; set; }
        public string AddressAr { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Email { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencySymbol { get; set; }
        public string FacebookUrl { get; set; }
        public string InstagramUrl { get; set; }
        public string TwitterUrl { get; set; }
        public string OpeningHoursEn { get; set; }
        public string OpeningHoursAr { get; set; }
        public decimal DeliveryFee { get; set; }
    }
}
