namespace MangaRestaurant.APIs.Dtos
{
    public class AdminReportDTO
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int PaymentReceivedOrders { get; set; }
        public int PaymentFailedOrders { get; set; }
        public decimal Revenue { get; set; }
    }
}