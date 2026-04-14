namespace MangaRestaurant.APIs.Dtos
{
    public class DeliveryLocationDto
    {
        public string OrderId  { get; set; } = string.Empty;
        public double Latitude  { get; set; }
        public double Longitude { get; set; }
        /// <summary>e.g. "on_the_way", "arrived", "delivered"</summary>
        public string Status   { get; set; } = "on_the_way";
    }
}
