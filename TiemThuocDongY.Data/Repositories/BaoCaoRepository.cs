using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using TiemThuocDongY.Data.Infrastructure;
using TiemThuocDongY.Domain.Entities;

namespace TiemThuocDongY.Data
{
    public class BaoCaoRepository
    {
        public BaoCaoTongHopThangDto GetTongHopThang(int year, int month)
        {
            var result = new BaoCaoTongHopThangDto
            {
                Year = year,
                Month = month
            };

            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1);

            var prevStart = start.AddMonths(-1);
            var prevEnd = start;

            var today = DateTime.Today;
            var sevenFrom = today.AddDays(-6).Date;
            var sevenTo = today.Date;

            using (var conn = Db.GetConnection())
            {
                conn.Open();

                // ---- 1. Doanh thu hôm nay, tháng này, tháng trước, nhập kho tháng ----
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT 
    ISNULL(SUM(CASE WHEN CAST(NgayLap AS date) = CAST(@Today AS date) 
                    THEN TienKhachPhaiTra ELSE 0 END), 0) AS RevToday,
    COUNT(CASE WHEN CAST(NgayLap AS date) = CAST(@Today AS date) THEN 1 END) AS OrdersToday,
    ISNULL(SUM(CASE WHEN NgayLap >= @Start AND NgayLap < @End 
                    THEN TienKhachPhaiTra ELSE 0 END), 0) AS RevMonth,
    COUNT(CASE WHEN NgayLap >= @Start AND NgayLap < @End THEN 1 END) AS OrdersMonth,
    ISNULL(SUM(CASE WHEN NgayLap >= @PrevStart AND NgayLap < @PrevEnd 
                    THEN TienKhachPhaiTra ELSE 0 END), 0) AS RevPrevMonth
FROM DonThuoc;

SELECT 
    ISNULL(SUM(CASE WHEN NgayNhap >= @Start AND NgayNhap < @End 
                    THEN TienPhaiTra ELSE 0 END), 0) AS ImportValue,
    COUNT(CASE WHEN NgayNhap >= @Start AND NgayNhap < @End THEN 1 END) AS ImportCount
FROM PhieuNhap;
";
                    cmd.Parameters.AddWithValue("@Today", today);
                    cmd.Parameters.AddWithValue("@Start", start);
                    cmd.Parameters.AddWithValue("@End", end);
                    cmd.Parameters.AddWithValue("@PrevStart", prevStart);
                    cmd.Parameters.AddWithValue("@PrevEnd", prevEnd);

                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        result.DoanhThuHomNay = reader.GetDecimal(0);
                        result.SoDonHomNay = reader.GetInt32(1);
                        result.DoanhThuThang = reader.GetDecimal(2);
                        result.SoDonThang = reader.GetInt32(3);
                        result.DoanhThuThangTruoc = reader.GetDecimal(4);
                    }

                    if (reader.NextResult() && reader.Read())
                    {
                        result.GiaTriNhapThang = reader.GetDecimal(0);
                        result.SoPhieuNhapThang = reader.GetInt32(1);
                    }
                }

                // ---- 2. Doanh thu 7 ngày gần nhất ----
                var byDate = new Dictionary<DateTime, (int SoDon, decimal DoanhThu)>();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT 
    CAST(NgayLap AS date) AS Ngay,
    COUNT(*) AS SoDon,
    ISNULL(SUM(TienKhachPhaiTra),0) AS DoanhThu
FROM DonThuoc
WHERE NgayLap >= @SevenFrom AND NgayLap < DATEADD(day,1,@SevenTo)
GROUP BY CAST(NgayLap AS date)
ORDER BY Ngay";
                    cmd.Parameters.AddWithValue("@SevenFrom", sevenFrom);
                    cmd.Parameters.AddWithValue("@SevenTo", sevenTo);

                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var d = reader.GetDateTime(0).Date;
                        var soDon = reader.GetInt32(1);
                        var rev = reader.GetDecimal(2);
                        byDate[d] = (soDon, rev);
                    }
                }

                for (var d = sevenFrom; d <= sevenTo; d = d.AddDays(1))
                {
                    if (!byDate.TryGetValue(d, out var v))
                        v = (0, 0);

                    result.DoanhThu7Ngay.Add(new BaoCao7NgayItem
                    {
                        Ngay = d,
                        SoDon = v.SoDon,
                        DoanhThu = v.DoanhThu
                    });
                }

                // ---- 3. Top thuốc bán chạy trong tháng ----
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT TOP 10
    t.ThuocId,
    t.TenThuoc,
    t.DonViTinh,
    SUM(ct.SoThang * ct.LieuLuongGram) AS SoLuongBan,
    SUM(ct.ThanhTien) AS DoanhThu
FROM DonThuocChiTiet ct
INNER JOIN DonThuoc dt ON dt.DonThuocId = ct.DonThuocId
INNER JOIN DM_Thuoc t ON t.ThuocId = ct.ThuocId
WHERE dt.NgayLap >= @Start AND dt.NgayLap < @End
GROUP BY t.ThuocId, t.TenThuoc, t.DonViTinh
ORDER BY SoLuongBan DESC, DoanhThu DESC";
                    cmd.Parameters.AddWithValue("@Start", start);
                    cmd.Parameters.AddWithValue("@End", end);

                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        result.TopThuocThang.Add(new BaoCaoTopThuocItem
                        {
                            ThuocId = reader.GetInt32(0),
                            TenThuoc = reader.GetString(1),
                            DonViTinh = reader.GetString(2),
                            SoLuongBan = reader.GetDecimal(3),
                            DoanhThu = reader.GetDecimal(4)
                        });
                    }
                }

                // ---- 4. Tổng hợp công nợ khách hàng / nhà cung cấp ----
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT 
    ISNULL(SUM(TongNo),0) AS TongNo,
    COUNT(CASE WHEN TongNo > 0 THEN 1 END) AS SoKhach
FROM v_CongNoKhachHang
WHERE TongNo > 0;

SELECT 
    ISNULL(SUM(TongNo),0) AS TongNo,
    COUNT(CASE WHEN TongNo > 0 THEN 1 END) AS SoNcc
FROM v_CongNoNhaCungCap
WHERE TongNo > 0;
";
                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        result.TongPhaiThuKhachHang = reader.GetDecimal(0);
                        result.SoKhachConNo = reader.GetInt32(1);
                    }

                    if (reader.NextResult() && reader.Read())
                    {
                        result.TongPhaiTraNhaCungCap = reader.GetDecimal(0);
                        result.SoNccConNo = reader.GetInt32(1);
                    }
                }

                // Chi tiết công nợ khách hàng
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT TOP 50
    KhachHangId,
    MaKhachHang,
    HoTen,
    TongNo,
    SoDonConNo,
    HanThanhToanGanNhat
FROM v_CongNoKhachHang
WHERE TongNo > 0
ORDER BY TongNo DESC";
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        result.CongNoKhachHang.Add(new BaoCaoCongNoKhachHangItem
                        {
                            KhachHangId = reader.GetInt32(0),
                            MaKhachHang = reader.GetString(1),
                            HoTen = reader.GetString(2),
                            TongNo = reader.GetDecimal(3),
                            SoDonConNo = reader.GetInt32(4),
                            HanThanhToanGanNhat = reader.IsDBNull(5)
                                ? (DateTime?)null
                                : reader.GetDateTime(5)
                        });
                    }
                }

                // Chi tiết công nợ nhà cung cấp
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT TOP 50
    NhaCungCapId,
    MaNCC,
    TenNCC,
    TongNo,
    SoPhieuConNo,
    HanThanhToanGanNhat
FROM v_CongNoNhaCungCap
WHERE TongNo > 0
ORDER BY TongNo DESC";
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        result.CongNoNhaCungCap.Add(new BaoCaoCongNoNhaCungCapItem
                        {
                            NhaCungCapId = reader.GetInt32(0),
                            MaNCC = reader.GetString(1),
                            TenNCC = reader.GetString(2),
                            TongNo = reader.GetDecimal(3),
                            SoPhieuConNo = reader.GetInt32(4),
                            HanThanhToanGanNhat = reader.IsDBNull(5)
                                ? (DateTime?)null
                                : reader.GetDateTime(5)
                        });
                    }
                }
            }

            return result;
        }
    }
}
