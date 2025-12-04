using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using TiemThuocDongY.Data.Infrastructure;
using TiemThuocDongY.Domain.Entities;

namespace TiemThuocDongY.Data
{
    public class DashboardRepository
    {
        public DashboardSummaryDto GetSummary(DateTime today, int activityLimit = 20)
        {
            var result = new DashboardSummaryDto();

            using (var conn = Db.GetConnection())
            {
                conn.Open();

                // 1. Doanh thu & số đơn hôm nay
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT 
    ISNULL(SUM(TienKhachPhaiTra), 0) AS RevenueToday,
    COUNT(*) AS OrdersToday
FROM DonThuoc
WHERE CAST(NgayLap AS date) = CAST(@Today AS date);

SELECT COUNT(*) AS PendingOrders
FROM DonThuoc
WHERE ConNo > 0;

SELECT COUNT(*) AS LowStockCount
FROM DM_Thuoc
WHERE SoLuongTon <= TonToiThieu;
";
                    cmd.Parameters.AddWithValue("@Today", today);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result.RevenueToday = reader.GetDecimal(0);
                            result.OrdersToday = reader.GetInt32(1);
                        }

                        if (reader.NextResult() && reader.Read())
                        {
                            result.PendingOrders = reader.GetInt32(0);
                        }

                        if (reader.NextResult() && reader.Read())
                        {
                            result.LowStockCount = reader.GetInt32(0);
                        }
                    }
                }

                // 2. Hoạt động gần đây: đơn thuốc, nhập kho, khách hàng
                var activities = new List<DashboardActivityItem>();

                // ===== ĐƠN THUỐC =====
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT TOP 15 
    NgayLap,
    SoDon,
    KhachHangId,
    CreatedBy
FROM DonThuoc
ORDER BY NgayLap DESC";

                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var time = reader.GetDateTime(0);
                        var soDon = reader.GetString(1);
                        var khId = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);

                        string actor;
                        if (reader.IsDBNull(3))
                            actor = "Hệ thống";
                        else
                            actor = reader.GetValue(3)?.ToString() ?? "Hệ thống";

                        activities.Add(new DashboardActivityItem
                        {
                            Time = time,
                            Type = "Đơn thuốc",
                            Description = $"Tạo đơn {soDon} (KH #{khId})",
                            Actor = string.IsNullOrWhiteSpace(actor) ? "Hệ thống" : actor
                        });
                    }
                }

                // ===== PHIẾU NHẬP KHO =====
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT TOP 15 
    pn.NgayNhap,
    pn.SoPhieu,
    ncc.TenNCC,
    pn.CreatedBy
FROM PhieuNhap pn
LEFT JOIN DM_NhaCungCap ncc ON pn.NhaCungCapId = ncc.NhaCungCapId
ORDER BY pn.NgayNhap DESC";

                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var time = reader.GetDateTime(0);
                        var soPhieu = reader.GetString(1);
                        var tenNcc = reader.IsDBNull(2) ? "" : reader.GetString(2);

                        string actor;
                        if (reader.IsDBNull(3))
                            actor = "Hệ thống";
                        else
                            actor = reader.GetValue(3)?.ToString() ?? "Hệ thống";

                        activities.Add(new DashboardActivityItem
                        {
                            Time = time,
                            Type = "Nhập kho",
                            Description = $"Nhập phiếu {soPhieu} từ {tenNcc}",
                            Actor = string.IsNullOrWhiteSpace(actor) ? "Hệ thống" : actor
                        });
                    }
                }

                // ===== KHÁCH HÀNG =====
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT TOP 15
    CreatedDate,
    MaKhachHang,
    HoTen,
    CreatedBy
FROM DM_KhachHang
ORDER BY CreatedDate DESC";

                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var time = reader.GetDateTime(0);
                        var maKh = reader.GetString(1);
                        var hoTen = reader.IsDBNull(2) ? "" : reader.GetString(2);

                        string actor;
                        if (reader.IsDBNull(3))
                            actor = "Hệ thống";
                        else
                            actor = reader.GetValue(3)?.ToString() ?? "Hệ thống";

                        activities.Add(new DashboardActivityItem
                        {
                            Time = time,
                            Type = "Khách hàng",
                            Description = $"Cập nhật khách hàng {maKh} - {hoTen}",
                            Actor = string.IsNullOrWhiteSpace(actor) ? "Hệ thống" : actor
                        });
                    }
                }

                result.RecentActivities = activities
                    .OrderByDescending(x => x.Time)
                    .Take(activityLimit)
                    .ToList();
            }

            return result;
        }
    }
}
