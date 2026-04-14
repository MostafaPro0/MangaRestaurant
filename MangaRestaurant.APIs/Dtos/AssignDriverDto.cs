namespace MangaRestaurant.APIs.Dtos
{
    public class AssignDriverDto
    {
        /// <summary>UserId of the delivery person to assign</summary>
        public string DeliveryPersonId   { get; set; } = string.Empty;
        public string DeliveryPersonName { get; set; } = string.Empty;
    }
}
