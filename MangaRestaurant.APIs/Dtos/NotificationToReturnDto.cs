using System;

namespace MangaRestaurant.APIs.Dtos
{
    public class NotificationToReturnDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string TitleAr { get; set; }
        public string Message { get; set; }
        public string MessageAr { get; set; }
        public string Type { get; set; }
        public string RelatedId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
