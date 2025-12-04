using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TiemThuocDongY.Data.Infrastructure;
using TiemThuocDongY.Domain.Entities;

namespace TiemThuocDongY.Data.Repositories
{
    public class DonThuocRepository
    {
        public IList<DonThuocListItem> GetAllForListing()
        {
            var list = new List<DonThuocListItem>();

            using var conn = Db.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
SELECT  dt.DonThuocId,
        dt.SoDon,
        dt.NgayLap,
        dt.KhachHangId,
        kh.MaKhachHang,
        kh.HoTen,
        dt.BacSiKeDon,
        dt.TongTienHang,
        dt.GiamGia,
        dt.TienKhachPhaiTra,
        dt.DaThanhToan,
        dt.ConNo,
        dt.HanThanhToan,
        dt.TrangThaiDon,
        dt.TrangThaiThanhToan
FROM    DonThuoc dt
LEFT JOIN DM_KhachHang kh ON kh.KhachHangId = dt.KhachHangId
ORDER BY dt.NgayLap DESC, dt.DonThuocId DESC;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var item = new DonThuocListItem
                {
                    DonThuocId = reader.GetInt32(reader.GetOrdinal("DonThuocId")),
                    SoDon = reader["SoDon"]?.ToString() ?? string.Empty,
                    NgayLap = reader.GetDateTime(reader.GetOrdinal("NgayLap")),
                    KhachHangId = reader.GetInt32(reader.GetOrdinal("KhachHangId")),
                    MaKhachHang = reader["MaKhachHang"]?.ToString() ?? string.Empty,
                    TenKhachHang = reader["HoTen"]?.ToString() ?? string.Empty,
                    BacSiKeDon = reader["BacSiKeDon"] == DBNull.Value
                        ? string.Empty
                        : reader["BacSiKeDon"].ToString(),
                    TongTienHang = reader.GetDecimal(reader.GetOrdinal("TongTienHang")),
                    GiamGia = reader.GetDecimal(reader.GetOrdinal("GiamGia")),
                    TienKhachPhaiTra = reader.GetDecimal(reader.GetOrdinal("TienKhachPhaiTra")),
                    DaThanhToan = reader.GetDecimal(reader.GetOrdinal("DaThanhToan")),
                    ConNo = reader.GetDecimal(reader.GetOrdinal("ConNo")),
                    HanThanhToan = reader.IsDBNull(reader.GetOrdinal("HanThanhToan"))
                        ? (DateTime?)null
                        : reader.GetDateTime(reader.GetOrdinal("HanThanhToan")),
                    TrangThaiDon = reader.GetByte(reader.GetOrdinal("TrangThaiDon")),
                    TrangThaiThanhToan = reader.GetByte(reader.GetOrdinal("TrangThaiThanhToan"))
                };

                list.Add(item);
            }

            return list;
        }

        public DonThuocDetailDto GetDetail(int donThuocId)
        {
            DonThuocDetailDto result = null;

            using var conn = Db.GetConnection();
            conn.Open();

            // Header
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"
SELECT  dt.DonThuocId,
        dt.SoDon,
        dt.NgayLap,
        dt.KhachHangId,
        kh.MaKhachHang,
        kh.HoTen,
        kh.NamSinh,
        kh.DiaChi,
        dt.BacSiKeDon,
        dt.ChanDoan,
        dt.TongTienHang,
        dt.GiamGia,
        dt.TienKhachPhaiTra,
        dt.DaThanhToan,
        dt.ConNo,
        dt.HanThanhToan,
        dt.TrangThaiDon,
        dt.TrangThaiThanhToan,
        dt.GhiChu
FROM    DonThuoc dt
LEFT JOIN DM_KhachHang kh ON kh.KhachHangId = dt.KhachHangId
WHERE   dt.DonThuocId = @DonThuocId;";

                cmd.Parameters.AddWithValue("@DonThuocId", donThuocId);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    result = new DonThuocDetailDto
                    {
                        DonThuocId = reader.GetInt32(reader.GetOrdinal("DonThuocId")),
                        SoDon = reader["SoDon"]?.ToString() ?? string.Empty,
                        NgayLap = reader.GetDateTime(reader.GetOrdinal("NgayLap")),
                        KhachHangId = reader.GetInt32(reader.GetOrdinal("KhachHangId")),
                        MaKhachHang = reader["MaKhachHang"]?.ToString() ?? string.Empty,
                        TenKhachHang = reader["HoTen"]?.ToString() ?? string.Empty,
                        NamSinh = reader.IsDBNull(reader.GetOrdinal("NamSinh"))
                            ? (int?)null
                            : Convert.ToInt32(reader["NamSinh"]),
                        DiaChi = reader["DiaChi"]?.ToString() ?? string.Empty,
                        BacSiKeDon = reader["BacSiKeDon"] == DBNull.Value
                            ? string.Empty
                            : reader["BacSiKeDon"].ToString(),
                        ChanDoan = reader["ChanDoan"] == DBNull.Value
                            ? string.Empty
                            : reader["ChanDoan"].ToString(),
                        TongTienHang = reader.GetDecimal(reader.GetOrdinal("TongTienHang")),
                        GiamGia = reader.GetDecimal(reader.GetOrdinal("GiamGia")),
                        TienKhachPhaiTra = reader.GetDecimal(reader.GetOrdinal("TienKhachPhaiTra")),
                        DaThanhToan = reader.GetDecimal(reader.GetOrdinal("DaThanhToan")),
                        ConNo = reader.GetDecimal(reader.GetOrdinal("ConNo")),
                        HanThanhToan = reader.IsDBNull(reader.GetOrdinal("HanThanhToan"))
                            ? (DateTime?)null
                            : reader.GetDateTime(reader.GetOrdinal("HanThanhToan")),
                        TrangThaiDon = reader.GetByte(reader.GetOrdinal("TrangThaiDon")),
                        TrangThaiThanhToan = reader.GetByte(reader.GetOrdinal("TrangThaiThanhToan")),
                        GhiChu = reader["GhiChu"]?.ToString() ?? string.Empty,
                        ChiTiet = new List<DonThuocDetailLineDto>()
                    };
                }
            }

            if (result == null)
                return null;

            // Details
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"
SELECT  ct.DonThuocChiTietId,
        ct.ThuocId,
        t.TenThuoc,
        ct.LieuLuongGram,
        ct.SoThang,
        ct.DonGiaBan,
        ct.ThanhTien,
        ct.GhiChu
FROM    DonThuocChiTiet ct
LEFT JOIN DM_Thuoc t ON t.ThuocId = ct.ThuocId
WHERE   ct.DonThuocId = @DonThuocId
ORDER BY ct.DonThuocChiTietId;";

                cmd.Parameters.AddWithValue("@DonThuocId", donThuocId);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var line = new DonThuocDetailLineDto
                    {
                        DonThuocChiTietId = reader.GetInt32(reader.GetOrdinal("DonThuocChiTietId")),
                        ThuocId = reader.GetInt32(reader.GetOrdinal("ThuocId")),
                        TenThuoc = reader["TenThuoc"]?.ToString() ?? string.Empty,
                        LieuLuongGram = reader.GetDecimal(reader.GetOrdinal("LieuLuongGram")),
                        SoThang = reader.GetInt32(reader.GetOrdinal("SoThang")),
                        DonGiaBan = reader.GetDecimal(reader.GetOrdinal("DonGiaBan")),
                        ThanhTien = reader.GetDecimal(reader.GetOrdinal("ThanhTien")),
                        GhiChu = reader["GhiChu"]?.ToString() ?? string.Empty
                    };

                    result.ChiTiet.Add(line);
                }
            }

            return result;
        }
        private string GenerateSoPhieu(SqlConnection conn, SqlTransaction tran)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tran;
            cmd.CommandText = "SELECT TOP 1 SoPhieu FROM PhieuThu ORDER BY PhieuThuId DESC";

            var last = cmd.ExecuteScalar() as string;
            if (string.IsNullOrEmpty(last))
                return "PT0001";

            if (!int.TryParse(last.Replace("PT", ""), out var num))
                num = 0;

            return "PT" + (num + 1).ToString("0000");
        }
        public void ThuTienDonThuoc(int donThuocId, decimal soTien, byte hinhThucThanhToan, string ghiChu)
        {
            using var conn = Db.GetConnection();
            conn.Open();
            using var tran = conn.BeginTransaction();

            try
            {
                // 1. Lấy thông tin đơn
                int khachHangId;
                decimal tongTienHang, giamGia, daThanhToanCu;

                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tran;
                    cmd.CommandText = @"
SELECT KhachHangId, TongTienHang, GiamGia, DaThanhToan
FROM DonThuoc
WHERE DonThuocId = @Id";
                    cmd.Parameters.AddWithValue("@Id", donThuocId);

                    using var reader = cmd.ExecuteReader();
                    if (!reader.Read())
                        throw new Exception("Không tìm thấy đơn thuốc.");

                    khachHangId = reader.GetInt32(0);
                    tongTienHang = reader.GetDecimal(1);
                    giamGia = reader.GetDecimal(2);
                    daThanhToanCu = reader.GetDecimal(3);
                }

                var daThanhToanMoi = daThanhToanCu + soTien;
                var tienPhaiTra = tongTienHang - giamGia;
                if (tienPhaiTra < 0) tienPhaiTra = 0;

                byte trangThaiThanhToan;
                if (daThanhToanMoi <= 0)
                    trangThaiThanhToan = 0; // chưa thanh toán
                else if (daThanhToanMoi < tienPhaiTra)
                    trangThaiThanhToan = 1; // trả một phần
                else
                    trangThaiThanhToan = 2; // đã thanh toán

                // 2. Tạo số phiếu thu
                var soPhieu = GenerateSoPhieu(conn, tran);

                using (var cmdInsert = conn.CreateCommand())
                {
                    cmdInsert.Transaction = tran;
                    cmdInsert.CommandText = @"
INSERT INTO PhieuThu
(SoPhieu, NgayThu, KhachHangId, DonThuocId, SoTien,
 HinhThucThanhToan, GhiChu, CreatedDate, CreatedBy)
VALUES
(@SoPhieu, @NgayThu, @KhachHangId, @DonThuocId, @SoTien,
 @HinhThucThanhToan, @GhiChu, GETDATE(), @CreatedBy);";

                    cmdInsert.Parameters.AddWithValue("@SoPhieu", soPhieu);
                    cmdInsert.Parameters.AddWithValue("@NgayThu", DateTime.Now);
                    cmdInsert.Parameters.AddWithValue("@KhachHangId", khachHangId);
                    cmdInsert.Parameters.AddWithValue("@DonThuocId", donThuocId);
                    cmdInsert.Parameters.AddWithValue("@SoTien", soTien);
                    cmdInsert.Parameters.AddWithValue("@HinhThucThanhToan", hinhThucThanhToan);
                    cmdInsert.Parameters.AddWithValue("@GhiChu", ghiChu ?? "");
                    cmdInsert.Parameters.AddWithValue("@CreatedBy", DBNull.Value);

                    cmdInsert.ExecuteNonQuery();
                }

                // 3. Cập nhật đơn
                using (var cmdUpdate = conn.CreateCommand())
                {
                    cmdUpdate.Transaction = tran;
                    cmdUpdate.CommandText = @"
UPDATE DonThuoc
SET DaThanhToan = @DaThanhToan,
    TrangThaiThanhToan = @TrangThaiThanhToan
WHERE DonThuocId = @Id";

                    cmdUpdate.Parameters.AddWithValue("@DaThanhToan", daThanhToanMoi);
                    cmdUpdate.Parameters.AddWithValue("@TrangThaiThanhToan", trangThaiThanhToan);
                    cmdUpdate.Parameters.AddWithValue("@Id", donThuocId);

                    cmdUpdate.ExecuteNonQuery();
                }

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }




        public string GenerateSoDon()
        {
            using var conn = Db.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT TOP 1 SoDon FROM DonThuoc ORDER BY DonThuocId DESC";

            var last = cmd.ExecuteScalar()?.ToString();
            if (string.IsNullOrEmpty(last))
                return "DT0001";

            int num = int.Parse(last.Replace("DT", ""));
            return "DT" + (num + 1).ToString("0000");
        }
        public int Insert(DonThuoc header, List<DonThuocChiTiet> details)
        {
            using var conn = Db.GetConnection();
            conn.Open();
            using var tran = conn.BeginTransaction();

            try
            {
                var cmd = conn.CreateCommand();
                cmd.Transaction = tran;
                cmd.CommandText = @"
INSERT INTO DonThuoc
(SoDon, NgayLap, KhachHangId, BacSiKeDon, ChanDoan,
 TongTienHang, GiamGia, DaThanhToan, TrangThaiDon,
 TrangThaiThanhToan,HanThanhToan, GhiChu, CreatedDate, CreatedBy)
OUTPUT INSERTED.DonThuocId
VALUES
(@SoDon, @NgayLap, @KhachHangId, @BacSiKeDon, @ChanDoan,
 @TongTienHang, @GiamGia, @DaThanhToan, @TrangThaiDon,
 @TrangThaiThanhToan,@HanThanhToan, @GhiChu, GETDATE(), @CreatedBy);";

                cmd.Parameters.AddWithValue("@SoDon", header.SoDon);
                cmd.Parameters.AddWithValue("@NgayLap", header.NgayLap);
                cmd.Parameters.AddWithValue("@KhachHangId", header.KhachHangId);
                cmd.Parameters.AddWithValue("@BacSiKeDon", header.BacSiKeDon ?? "");
                cmd.Parameters.AddWithValue("@ChanDoan", header.ChanDoan ?? "");
                cmd.Parameters.AddWithValue("@TongTienHang", header.TongTienHang);
                cmd.Parameters.AddWithValue("@GiamGia", header.GiamGia);

                // tạo đơn thì đã thanh toán = 0
                cmd.Parameters.AddWithValue("@DaThanhToan", header.DaThanhToan);

                cmd.Parameters.AddWithValue("@TrangThaiDon", 0);
                cmd.Parameters.AddWithValue("@TrangThaiThanhToan", header.TrangThaiThanhToan);

                // 🔹 thêm dòng này
                cmd.Parameters.AddWithValue(
                    "@HanThanhToan",
                    (object?)header.HanThanhToan ?? DBNull.Value
                );

                cmd.Parameters.AddWithValue("@GhiChu", header.GhiChu ?? "");
                cmd.Parameters.AddWithValue("@CreatedBy", header.CreatedBy);

                int newId = (int)cmd.ExecuteScalar();

                // ===== Insert chi tiết =====
                foreach (var ct in details)
                {
                    var cmd2 = conn.CreateCommand();
                    cmd2.Transaction = tran;
                    cmd2.CommandText = @"
INSERT INTO DonThuocChiTiet
(DonThuocId, ThuocId, LieuLuongGram, SoThang, DonGiaBan, ThanhTien, GhiChu)
VALUES (@DonThuocId, @ThuocId, @Lieu, @SoThang, @DonGia, @ThanhTien, @GhiChu)";
                    cmd2.Parameters.AddWithValue("@DonThuocId", newId);
                    cmd2.Parameters.AddWithValue("@ThuocId", ct.ThuocId);
                    cmd2.Parameters.AddWithValue("@Lieu", ct.LieuLuongGram);
                    cmd2.Parameters.AddWithValue("@SoThang", ct.SoThang);
                    cmd2.Parameters.AddWithValue("@DonGia", ct.DonGiaBan);
                    cmd2.Parameters.AddWithValue("@ThanhTien", ct.ThanhTien);
                    cmd2.Parameters.AddWithValue("@GhiChu", ct.GhiChu ?? "");
                    cmd2.ExecuteNonQuery();
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

        public void UpdateTrangThaiDon(DonThuoc h)
        {
            using (var conn = Db.GetConnection())
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
UPDATE DonThuoc
SET TrangThaiDon = @TrangThaiDon
WHERE DonThuocId = @Id;";

                    cmd.Parameters.AddWithValue("@TrangThaiDon", h.TrangThaiDon);
                    cmd.Parameters.AddWithValue("@Id", h.DonThuocId);

                    cmd.ExecuteNonQuery();
                }
            }
        }
        public void Delete(int id)
        {
            using var conn = Db.GetConnection();
            conn.Open();

            using var cmdCheck = conn.CreateCommand();
            cmdCheck.CommandText = "SELECT COUNT(*) FROM PhieuThu WHERE DonThuocId = @Id";
            cmdCheck.Parameters.AddWithValue("@Id", id);
            var count = (int)cmdCheck.ExecuteScalar();

            if (count > 0)
            {
                throw new Exception("Đơn thuốc này đã có phiếu thu, không thể xóa.");
            }

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM DonThuoc WHERE DonThuocId = @Id";
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }


        public void Update(DonThuoc header, List<DonThuocChiTiet> details)
        {
            using var conn = Db.GetConnection();
            conn.Open();
            using var tran = conn.BeginTransaction();

            try
            {
                // Update header (KHÔNG cập nhật 2 cột computed)
                var cmd = conn.CreateCommand();
                cmd.Transaction = tran;
                cmd.CommandText = @"
UPDATE DonThuoc SET
    KhachHangId=@KhachHangId,
    NgayLap=@NgayLap,
    BacSiKeDon=@BacSiKeDon,
    ChanDoan=@ChanDoan,
    TongTienHang=@TongTienHang,
    GiamGia=@GiamGia,
    GhiChu=@GhiChu
WHERE DonThuocId=@DonThuocId";

                cmd.Parameters.AddWithValue("@DonThuocId", header.DonThuocId);
                cmd.Parameters.AddWithValue("@KhachHangId", header.KhachHangId);
                cmd.Parameters.AddWithValue("@NgayLap", header.NgayLap);
                cmd.Parameters.AddWithValue("@BacSiKeDon", header.BacSiKeDon ?? "");
                cmd.Parameters.AddWithValue("@ChanDoan", header.ChanDoan ?? "");
                cmd.Parameters.AddWithValue("@TongTienHang", header.TongTienHang);
                cmd.Parameters.AddWithValue("@GiamGia", header.GiamGia);
                cmd.Parameters.AddWithValue("@GhiChu", header.GhiChu ?? "");


                cmd.ExecuteNonQuery();

                // Xóa toàn bộ chi tiết cũ
                var cmdDel = conn.CreateCommand();
                cmdDel.Transaction = tran;
                cmdDel.CommandText = "DELETE FROM DonThuocChiTiet WHERE DonThuocId=@id";
                cmdDel.Parameters.AddWithValue("@id", header.DonThuocId);
                cmdDel.ExecuteNonQuery();

                // Insert chi tiết mới
                foreach (var ct in details)
                {
                    var cmd2 = conn.CreateCommand();
                    cmd2.Transaction = tran;
                    cmd2.CommandText = @"
INSERT INTO DonThuocChiTiet
(DonThuocId, ThuocId, LieuLuongGram, SoThang, DonGiaBan, ThanhTien, GhiChu)
VALUES (@DonThuocId, @ThuocId, @Lieu, @SoThang, @DonGia, @ThanhTien, @GhiChu)";
                    cmd2.Parameters.AddWithValue("@DonThuocId", header.DonThuocId);
                    cmd2.Parameters.AddWithValue("@ThuocId", ct.ThuocId);
                    cmd2.Parameters.AddWithValue("@Lieu", ct.LieuLuongGram);
                    cmd2.Parameters.AddWithValue("@SoThang", ct.SoThang);
                    cmd2.Parameters.AddWithValue("@DonGia", ct.DonGiaBan);
                    cmd2.Parameters.AddWithValue("@ThanhTien", ct.ThanhTien);
                    cmd2.Parameters.AddWithValue("@GhiChu", ct.GhiChu ?? "");
                    cmd2.ExecuteNonQuery();
                }

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }
        public void MarkAsCompleted(int id)
        {
            using var conn = Db.GetConnection();
            conn.Open();
            using var tran = conn.BeginTransaction();

            try
            {
                // 1. Kiểm tra trạng thái hiện tại, tránh trừ kho 2 lần
                byte? trangThaiHienTai = null;
                using (var cmdCheck = conn.CreateCommand())
                {
                    cmdCheck.Transaction = tran;
                    cmdCheck.CommandText = @"
SELECT TrangThaiDon
FROM DonThuoc
WHERE DonThuocId = @Id";
                    cmdCheck.Parameters.AddWithValue("@Id", id);

                    var obj = cmdCheck.ExecuteScalar();
                    if (obj == null)
                        throw new Exception("Không tìm thấy đơn thuốc.");

                    trangThaiHienTai = Convert.ToByte(obj);
                }

                if (trangThaiHienTai == 1)
                {
                    // đã là "Đã kê đơn" rồi thì thôi, không trừ kho nữa
                    tran.Commit();
                    return;
                }

                // 2. Lấy chi tiết đơn và gom lượng cần trừ theo ThuocId
                var canTru = new Dictionary<int, decimal>(); // ThuocId -> tổng gram

                using (var cmdLines = conn.CreateCommand())
                {
                    cmdLines.Transaction = tran;
                    cmdLines.CommandText = @"
SELECT ThuocId, LieuLuongGram, SoThang
FROM DonThuocChiTiet
WHERE DonThuocId = @Id";
                    cmdLines.Parameters.AddWithValue("@Id", id);

                    using var reader = cmdLines.ExecuteReader();
                    while (reader.Read())
                    {
                        int thuocId = reader.GetInt32(0);
                        decimal lieuGram = reader.GetDecimal(1);
                        int soThang = reader.GetInt32(2);

                        decimal soLuongCanTru = lieuGram * soThang; // tổng gram

                        if (canTru.TryGetValue(thuocId, out var cur))
                            canTru[thuocId] = cur + soLuongCanTru;
                        else
                            canTru[thuocId] = soLuongCanTru;
                    }
                }

                // 3. Kiểm tra tồn kho từng thuốc
                foreach (var kv in canTru)
                {
                    int thuocId = kv.Key;
                    decimal soLuongCanTru = kv.Value;

                    string tenThuoc;
                    decimal tonHienTai;

                    using (var cmdGetTon = conn.CreateCommand())
                    {
                        cmdGetTon.Transaction = tran;
                        cmdGetTon.CommandText = @"
SELECT SoLuongTon, TenThuoc
FROM DM_Thuoc
WHERE ThuocId = @ThuocId";
                        cmdGetTon.Parameters.AddWithValue("@ThuocId", thuocId);

                        using var reader = cmdGetTon.ExecuteReader();
                        if (!reader.Read())
                            throw new Exception($"Không tìm thấy thuốc (ID = {thuocId}).");

                        tonHienTai = reader.GetDecimal(0);
                        tenThuoc = reader["TenThuoc"]?.ToString() ?? "";
                    }

                    if (tonHienTai < soLuongCanTru)
                    {
                        throw new Exception(
                            $"Không đủ tồn kho cho thuốc '{tenThuoc}'. " +
                            $"Cần {soLuongCanTru}g, hiện chỉ còn {tonHienTai}g.");
                    }
                }

                // 4. Trừ tồn kho
                foreach (var kv in canTru)
                {
                    int thuocId = kv.Key;
                    decimal soLuongCanTru = kv.Value;

                    using var cmdUpdateTon = conn.CreateCommand();
                    cmdUpdateTon.Transaction = tran;
                    cmdUpdateTon.CommandText = @"
UPDATE DM_Thuoc
SET SoLuongTon = SoLuongTon - @Qty
WHERE ThuocId = @ThuocId";
                    cmdUpdateTon.Parameters.AddWithValue("@Qty", soLuongCanTru);
                    cmdUpdateTon.Parameters.AddWithValue("@ThuocId", thuocId);

                    cmdUpdateTon.ExecuteNonQuery();
                }

                // 5. Cập nhật trạng thái đơn sang ĐÃ KÊ ĐƠN
                using (var cmdUpdateStatus = conn.CreateCommand())
                {
                    cmdUpdateStatus.Transaction = tran;
                    cmdUpdateStatus.CommandText = @"
UPDATE DonThuoc
SET TrangThaiDon = 1
WHERE DonThuocId = @Id";
                    cmdUpdateStatus.Parameters.AddWithValue("@Id", id);
                    cmdUpdateStatus.ExecuteNonQuery();
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
