using System;
using System.Collections.Generic;

namespace TiemThuocDongY.Domain.Entities
{
    public class DashboardActivityItem
    {
        public DateTime Time { get; set; }
        public string Type { get; set; } = "";
        public string Description { get; set; } = "";
        public string Actor { get; set; } = "";
    }

    public class DashboardSummaryDto
    {
        public decimal RevenueToday { get; set; }
        public int OrdersToday { get; set; }
        public int PendingOrders { get; set; }
        public int LowStockCount { get; set; }

        public List<DashboardActivityItem> RecentActivities { get; set; } = new();
    }
}
