using System.Collections.Generic;

namespace SuperShop_Management.DTOs.Dashboard
{
    public class RecentActivityDto
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class DashboardResponseDto
    {
        public int TotalProducts { get; set; }
        public int TotalSuppliers { get; set; }
        public int TotalBatches { get; set; }
        public int TotalDepartments { get; set; }
        public int TotalPurchaseOrders { get; set; }
        public int LowStockItems { get; set; }
        public List<RecentActivityDto> RecentActivities { get; set; } = new List<RecentActivityDto>();
    }
}
