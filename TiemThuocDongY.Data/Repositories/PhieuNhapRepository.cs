using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TiemThuocDongY.Data.Infrastructure;
using TiemThuocDongY.Domain.Entities;

namespace TiemThuocDongY.Data.Repositories
{
    public class PhieuNhapRepository
    {
        public IList<PhieuNhapListItem> GetAllForListing()
        {
            var list = new List<PhieuNhapListItem>();

            using var conn = Db.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
SELECT 
    pn.PhieuNhapId,
    pn.SoPhieu,
    pn.NgayNhap,
    pn.NhaCungCapId,
    ncc.MaNCC,
    ncc.TenNCC,
    COUNT(DISTINCT ct.PhieuNhapChiTietId) AS SoMatHang,
    ISNULL(SUM(ct.SoLuong),0)            AS TongSoLuong,
    pn.TongTienHang,
    pn.GiamGia,
    pn.TienPhaiTra,
    pn.DaThanhToan,
    pn.ConNo,
    pn.HanThanhToan,
    pn.TrangThaiThanhToan
FROM PhieuNhap pn
LEFT JOIN DM_NhaCungCap ncc ON ncc.NhaCungCapId = pn.NhaCungCapId
LEFT JOIN PhieuNhapChiTiet ct ON ct.PhieuNhapId = pn.PhieuNhapId
GROUP BY 
    pn.PhieuNhapId,
    pn.SoPhieu,
    pn.NgayNhap,
    pn.NhaCungCapId,
    ncc.MaNCC,
    ncc.TenNCC,
    pn.TongTienHang,
    pn.GiamGia,
    pn.TienPhaiTra,
    pn.DaThanhToan,
    pn.ConNo,
    pn.HanThanhToan,
    pn.TrangThaiThanhToan
ORDER BY pn.NgayNhap DESC, pn.PhieuNhapId DESC;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var item = new PhieuNhapListItem
                {
                    PhieuNhapId = reader.GetInt32(reader.GetOrdinal("PhieuNhapId")),
                    SoPhieu = reader["SoPhieu"]?.ToString() ?? "",
                    NgayNhap = reader.GetDateTime(reader.GetOrdinal("NgayNhap")),
                    NhaCungCapId = reader.GetInt32(reader.GetOrdinal("NhaCungCapId")),
                    MaNcc = reader["MaNCC"]?.ToString() ?? "",
                    TenNcc = reader["TenNCC"]?.ToString() ?? "",
                    SoMatHang = reader.GetInt32(reader.GetOrdinal("SoMatHang")),
                    TongSoLuong = reader.GetDecimal(reader.GetOrdinal("TongSoLuong")),
                    TongTienHang = reader.GetDecimal(reader.GetOrdinal("TongTienHang")),
                    GiamGia = reader.GetDecimal(reader.GetOrdinal("GiamGia")),
                    TienPhaiTra = reader.GetDecimal(reader.GetOrdinal("TienPhaiTra")),
                    DaThanhToan = reader.GetDecimal(reader.GetOrdinal("DaThanhToan")),
                    ConNo = reader.GetDecimal(reader.GetOrdinal("ConNo")),
                    HanThanhToan = reader.IsDBNull(reader.GetOrdinal("HanThanhToan"))
                        ? (DateTime?)null
                        : reader.GetDateTime(reader.GetOrdinal("HanThanhToan")),
                    TrangThaiThanhToan = reader.GetByte(reader.GetOrdinal("TrangThaiThanhToan"))
                };

                list.Add(item);
            }

            return list;
        }

        public PhieuNhapDetailDto GetDetail(int phieuNhapId)
        {
            PhieuNhapDetailDto result = null;

            using var conn = Db.GetConnection();
            conn.Open();

            // Header
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"
SELECT 
    pn.PhieuNhapId,
    pn.SoPhieu,
    pn.NgayNhap,
    pn.NhaCungCapId,
    ncc.MaNCC,
    ncc.TenNCC,
    ncc.NguoiLienHe,
    ncc.DiaChi,
    pn.TongTienHang,
    pn.GiamGia,
    pn.TienPhaiTra,
    pn.DaThanhToan,
    pn.ConNo,
    pn.HanThanhToan,
    pn.TrangThaiThanhToan,
    pn.GhiChu
FROM PhieuNhap pn
LEFT JOIN DM_NhaCungCap ncc ON ncc.NhaCungCapId = pn.NhaCungCapId
WHERE pn.PhieuNhapId = @Id;";

                cmd.Parameters.AddWithValue("@Id", phieuNhapId);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    result = new PhieuNhapDetailDto
                    {
                        PhieuNhapId = reader.GetInt32(reader.GetOrdinal("PhieuNhapId")),
                        SoPhieu = reader["SoPhieu"]?.ToString() ?? "",
                        NgayNhap = reader.GetDateTime(reader.GetOrdinal("NgayNhap")),
                        NhaCungCapId = reader.GetInt32(reader.GetOrdinal("NhaCungCapId")),
                        MaNcc = reader["MaNCC"]?.ToString() ?? "",
                        TenNcc = reader["TenNCC"]?.ToString() ?? "",
                        NguoiLienHe = reader["NguoiLienHe"]?.ToString() ?? "",
                        DiaChi = reader["DiaChi"]?.ToString() ?? "",
                        TongTienHang = reader.GetDecimal(reader.GetOrdinal("TongTienHang")),
                        GiamGia = reader.GetDecimal(reader.GetOrdinal("GiamGia")),
                        TienPhaiTra = reader.GetDecimal(reader.GetOrdinal("TienPhaiTra")),
                        DaThanhToan = reader.GetDecimal(reader.GetOrdinal("DaThanhToan")),
                        ConNo = reader.GetDecimal(reader.GetOrdinal("ConNo")),
                        HanThanhToan = reader.IsDBNull(reader.GetOrdinal("HanThanhToan"))
                            ? (DateTime?)null
                            : reader.GetDateTime(reader.GetOrdinal("HanThanhToan")),
                        TrangThaiThanhToan = reader.GetByte(reader.GetOrdinal("TrangThaiThanhToan")),
                        GhiChu = reader["GhiChu"]?.ToString() ?? "",
                        ChiTiet = new List<PhieuNhapDetailLineDto>()
                    };
                }
            }

            if (result == null) return null;

            // Details
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"
SELECT 
    ct.PhieuNhapChiTietId,
    ct.ThuocId,
    t.TenThuoc,
    ct.DonViTinh,
    ct.SoLuong,
    ct.DonGiaNhap,
    ct.ThanhTien,
    ct.GhiChu
FROM PhieuNhapChiTiet ct
LEFT JOIN DM_Thuoc t ON t.ThuocId = ct.ThuocId
WHERE ct.PhieuNhapId = @Id
ORDER BY ct.PhieuNhapChiTietId;";

                cmd.Parameters.AddWithValue("@Id", phieuNhapId);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    result.ChiTiet.Add(new PhieuNhapDetailLineDto
                    {
                        PhieuNhapChiTietId = reader.GetInt32(reader.GetOrdinal("PhieuNhapChiTietId")),
                        ThuocId = reader.GetInt32(reader.GetOrdinal("ThuocId")),
                        TenThuoc = reader["TenThuoc"]?.ToString() ?? "",
                        DonViTinh = reader["DonViTinh"]?.ToString() ?? "",
                        SoLuong = reader.GetDecimal(reader.GetOrdinal("SoLuong")),
                        DonGiaNhap = reader.GetDecimal(reader.GetOrdinal("DonGiaNhap")),
                        ThanhTien = reader.GetDecimal(reader.GetOrdinal("ThanhTien")),
                        GhiChu = reader["GhiChu"]?.ToString() ?? ""
                    });
                }
            }

            return result;
        }

        public string GenerateSoPhieu(SqlConnection conn, SqlTransaction tran)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tran;
            cmd.CommandText = @"SELECT TOP 1 SoPhieu FROM PhieuNhap ORDER BY PhieuNhapId DESC";
            var last = cmd.ExecuteScalar()?.ToString();
            if (string.IsNullOrEmpty(last))
                return "PN0001";

            // Ví dụ PN0001
            var numPart = last.Replace("PN", "");
            int num = int.TryParse(numPart, out var n) ? n : 0;
            return "PN" + (num + 1).ToString("0000");
        }

        public int Insert(PhieuNhapCreateHeaderDto header, List<PhieuNhapCreateLineDto> details, int currentUserId)
        {
            using var conn = Db.GetConnection();
            conn.Open();
            using var tran = conn.BeginTransaction();

            try
            {
                // đảm bảo có SoPhieu
                if (string.IsNullOrWhiteSpace(header.SoPhieu))
                {
                    header.SoPhieu = GenerateSoPhieu(conn, tran);
                }
                // 🔹 Nếu chưa có hạn thanh toán -> mặc định = Ngày nhập + 15 ngày
                if (!header.HanThanhToan.HasValue)
                {
                    header.HanThanhToan = header.NgayNhap.AddDays(15);
                }
                // tính lại tổng tiền
                decimal tongTienHang = 0;
                foreach (var d in details)
                    tongTienHang += d.ThanhTien;

                header.TongTienHang = tongTienHang;

                using var cmd = conn.CreateCommand();
                cmd.Transaction = tran;
                cmd.CommandText = @"
INSERT INTO PhieuNhap
(SoPhieu, NgayNhap, NhaCungCapId, TongTienHang, GiamGia,
 DaThanhToan, TrangThaiThanhToan, HanThanhToan, GhiChu, CreatedDate, CreatedBy)
OUTPUT INSERTED.PhieuNhapId
VALUES
(@SoPhieu, @NgayNhap, @NhaCungCapId, @TongTienHang, @GiamGia,
 @DaThanhToan, @TrangThaiThanhToan, @HanThanhToan, @GhiChu, GETDATE(), @CreatedBy);";

                cmd.Parameters.AddWithValue("@SoPhieu", header.SoPhieu);
                cmd.Parameters.AddWithValue("@NgayNhap", header.NgayNhap);
                cmd.Parameters.AddWithValue("@NhaCungCapId", header.NhaCungCapId);
                cmd.Parameters.AddWithValue("@TongTienHang", header.TongTienHang);
                cmd.Parameters.AddWithValue("@GiamGia", header.GiamGia);
                cmd.Parameters.AddWithValue("@DaThanhToan", 0m); // mới nhập kho chưa trả NCC
                cmd.Parameters.AddWithValue("@TrangThaiThanhToan", (byte)0); // 0 = chưa thanh toán
                cmd.Parameters.AddWithValue("@HanThanhToan", (object?)header.HanThanhToan ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@GhiChu", header.GhiChu ?? "");
                cmd.Parameters.AddWithValue("@CreatedBy", currentUserId);

                int newId = (int)cmd.ExecuteScalar();

                // chi tiết + update tồn kho
                foreach (var ct in details)
                {
                    using var cmd2 = conn.CreateCommand();
                    cmd2.Transaction = tran;
                    cmd2.CommandText = @"
INSERT INTO PhieuNhapChiTiet
(PhieuNhapId, ThuocId, DonViTinh, SoLuong, DonGiaNhap, ThanhTien, GhiChu)
VALUES (@PhieuNhapId, @ThuocId, @DonViTinh, @SoLuong, @DonGia, @ThanhTien, @GhiChu);";
                    cmd2.Parameters.AddWithValue("@PhieuNhapId", newId);
                    cmd2.Parameters.AddWithValue("@ThuocId", ct.ThuocId);
                    // đơn vị tính bạn lấy luôn từ DM_Thuoc cho chắc,
                    // tạm thời ghi 'gram' từ UI nếu có
                    cmd2.Parameters.AddWithValue("@DonViTinh", "gram");
                    cmd2.Parameters.AddWithValue("@SoLuong", ct.SoLuong);
                    cmd2.Parameters.AddWithValue("@DonGia", ct.DonGiaNhap);
                    cmd2.Parameters.AddWithValue("@ThanhTien", ct.ThanhTien);
                    cmd2.Parameters.AddWithValue("@GhiChu", ct.GhiChu ?? "");
                    cmd2.ExecuteNonQuery();

                    // tăng tồn kho
                    using var cmdStock = conn.CreateCommand();
                    cmdStock.Transaction = tran;
                    cmdStock.CommandText = @"
UPDATE DM_Thuoc
SET SoLuongTon = ISNULL(SoLuongTon,0) + @SoLuong
WHERE ThuocId = @ThuocId;";
                    cmdStock.Parameters.AddWithValue("@SoLuong", ct.SoLuong);
                    cmdStock.Parameters.AddWithValue("@ThuocId", ct.ThuocId);
                    cmdStock.ExecuteNonQuery();
                }

                tran.Commit();
                return newId;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public void AddPayment(int phieuNhapId, decimal soTien, byte hinhThuc, string ghiChu, int userId)
        {
            using var conn = Db.GetConnection();
            conn.Open();
            using var tran = conn.BeginTransaction();

            try
            {
                // Lấy thông tin phiếu nhập
                decimal daThanhToan = 0;
                decimal tienPhaiTra = 0;

                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tran;
                    cmd.CommandText = "SELECT DaThanhToan, TienPhaiTra FROM PhieuNhap WHERE PhieuNhapId = @Id";
                    cmd.Parameters.AddWithValue("@Id", phieuNhapId);
                    using var rd = cmd.ExecuteReader();
                    if (rd.Read())
                    {
                        daThanhToan = rd.GetDecimal(0);
                        tienPhaiTra = rd.GetDecimal(1);
                    }
                }

                var newPaid = daThanhToan + soTien;
                byte trangThai;
                if (newPaid <= 0) trangThai = 0;             // chưa thanh toán
                else if (newPaid < tienPhaiTra) trangThai = 2; // còn nợ
                else trangThai = 1;                           // đã thanh toán

                // Insert PhieuChi
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tran;
                    cmd.CommandText = @"
INSERT INTO PhieuChi
(SoPhieu, NgayChi, NhaCungCapId, PhieuNhapId, SoTien, HinhThucThanhToan, GhiChu, CreatedDate, CreatedBy)
VALUES (@SoPhieu, @NgayChi, 
        (SELECT NhaCungCapId FROM PhieuNhap WHERE PhieuNhapId=@PhieuNhapId),
        @PhieuNhapId, @SoTien, @HinhThuc, @GhiChu, GETDATE(), @CreatedBy);";

                    cmd.Parameters.AddWithValue("@SoPhieu", $"PC{DateTime.Now:yyyyMMddHHmmss}");
                    cmd.Parameters.AddWithValue("@NgayChi", DateTime.Now);
                    cmd.Parameters.AddWithValue("@PhieuNhapId", phieuNhapId);
                    cmd.Parameters.AddWithValue("@SoTien", soTien);
                    cmd.Parameters.AddWithValue("@HinhThuc", hinhThuc);
                    cmd.Parameters.AddWithValue("@GhiChu", ghiChu ?? "");
                    cmd.Parameters.AddWithValue("@CreatedBy", userId);
                    cmd.ExecuteNonQuery();
                }

                // Update lại PhieuNhap
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tran;
                    cmd.CommandText = @"
UPDATE PhieuNhap
SET DaThanhToan = DaThanhToan + @SoTien,
    TrangThaiThanhToan = @TrangThai
WHERE PhieuNhapId = @Id;";
                    cmd.Parameters.AddWithValue("@SoTien", soTien);
                    cmd.Parameters.AddWithValue("@TrangThai", trangThai);
                    cmd.Parameters.AddWithValue("@Id", phieuNhapId);
                    cmd.ExecuteNonQuery();
                }

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }
    }
}
