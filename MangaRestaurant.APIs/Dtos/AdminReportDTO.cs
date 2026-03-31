namespace MangaRestaurant.APIs.Dtos
{
    public class AdminReportDTO
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int PaymentReceivedOrders { get; set; }
        public int PaymentFailedOrders { get; set; }
        public decimal Revenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<DailySalesDTO> SalesLast7Days { get; set; } = new();
        public List<TopProductDTO> TopProducts { get; set; } = new();
        public List<TopCategoryDTO> TopCategories { get; set; } = new();
        public List<TopDeliveryDTO> TopDeliveryMethods { get; set; } = new();
        public List<PeakHourDTO> PeakHours { get; set; } = new();
        public List<TopEmployeeDTO> TopEmployees { get; set; } = new();
    }

    public class TopEmployeeDTO
    {
        public string Name { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class TopDeliveryDTO
    {
        public string Name { get; set; }
        public int Count { get; set; }
    }

    public class PeakHourDTO
    {
        public int Hour { get; set; }
        public int Count { get; set; }
    }

    public class DailySalesDTO
    {
        public string Date { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TopProductDTO
    {
        public string Name { get; set; }
        public string NameAr { get; set; }
        public int Quantity { get; set; }
    }

    public class TopCategoryDTO
    {
        public string Name { get; set; }
        public string NameAr { get; set; }
        public int Count { get; set; }
    }
}