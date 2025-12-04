using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TiemThuocDongY.Data.Infrastructure;
using TiemThuocDongY.Domain.Entities;

namespace TiemThuocDongY.Data.Repositories
{
    public class KhachHangRepository
    {
        public IList<DM_KhachHang> GetAll()
        {
            var list = new List<DM_KhachHang>();

            using var conn = Db.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
SELECT  KhachHangId,
        MaKhachHang,
        HoTen,
        NamSinh,
        GioiTinh,
        DienThoai,
        Email,
        DiaChi,
        GhiChu
FROM    DM_KhachHang
ORDER BY KhachHangId DESC;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var kh = new DM_KhachHang
                {
                    KhachHangId = reader.GetInt32(reader.GetOrdinal("KhachHangId")),
                    MaKhachHang = reader["MaKhachHang"]?.ToString() ?? "",
                    HoTen = reader["HoTen"]?.ToString() ?? "",
                    // NamSinh: SMALLINT -> int?
                    NamSinh = reader.IsDBNull(reader.GetOrdinal("NamSinh"))
                                  ? (int?)null
                                  : Convert.ToInt32(reader["NamSinh"]),
                    GioiTinh = reader.IsDBNull(reader.GetOrdinal("GioiTinh"))
                                  ? (byte?)null
                                  : reader.GetByte(reader.GetOrdinal("GioiTinh")),
                    DienThoai = reader["DienThoai"]?.ToString() ?? "",
                    Email = reader["Email"]?.ToString() ?? "",
                    DiaChi = reader["DiaChi"]?.ToString() ?? "",
                    GhiChu = reader["GhiChu"]?.ToString() ?? ""
                };

                list.Add(kh);
            }

            return list;
        }

        public int Insert(DM_KhachHang kh)
        {
            using var conn = Db.GetConnection();
            conn.Open();

            // 🔒 Luôn tự sinh mã KH: KH001, KH002, ...
            kh.MaKhachHang = GenerateMaKhachHang(conn);

            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
INSERT INTO DM_KhachHang
    (MaKhachHang, HoTen, NamSinh, GioiTinh, DienThoai, Email, DiaChi, GhiChu)
VALUES
    (@MaKhachHang, @HoTen, @NamSinh, @GioiTinh, @DienThoai, @Email, @DiaChi, @GhiChu);";

            cmd.Parameters.AddWithValue("@MaKhachHang", kh.MaKhachHang);
            cmd.Parameters.AddWithValue("@HoTen", kh.HoTen);

            if (kh.NamSinh.HasValue)
                cmd.Parameters.Add("@NamSinh", SqlDbType.SmallInt).Value = kh.NamSinh.Value;
            else
                cmd.Parameters.Add("@NamSinh", SqlDbType.SmallInt).Value = DBNull.Value;

            if (kh.GioiTinh.HasValue)
                cmd.Parameters.Add("@GioiTinh", SqlDbType.TinyInt).Value = kh.GioiTinh.Value;
            else
                cmd.Parameters.Add("@GioiTinh", SqlDbType.TinyInt).Value = DBNull.Value;

            cmd.Parameters.AddWithValue("@DienThoai", (object?)kh.DienThoai ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", (object?)kh.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DiaChi", (object?)kh.DiaChi ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GhiChu", (object?)kh.GhiChu ?? DBNull.Value);

            return cmd.ExecuteNonQuery();
        }

        public int Update(DM_KhachHang kh)
        {
            using var conn = Db.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
UPDATE DM_KhachHang
SET HoTen       = @HoTen,
    NamSinh     = @NamSinh,
    GioiTinh    = @GioiTinh,
    DienThoai   = @DienThoai,
    Email       = @Email,
    DiaChi      = @DiaChi,
    GhiChu      = @GhiChu
WHERE KhachHangId = @KhachHangId;";

            cmd.Parameters.AddWithValue("@KhachHangId", kh.KhachHangId);
            // ❌ Không cho chỉnh MaKhachHang nữa nên bỏ @MaKhachHang
            cmd.Parameters.AddWithValue("@HoTen", kh.HoTen);

            if (kh.NamSinh.HasValue)
                cmd.Parameters.Add("@NamSinh", SqlDbType.SmallInt).Value = kh.NamSinh.Value;
            else
                cmd.Parameters.Add("@NamSinh", SqlDbType.SmallInt).Value = DBNull.Value;

            if (kh.GioiTinh.HasValue)
                cmd.Parameters.Add("@GioiTinh", SqlDbType.TinyInt).Value = kh.GioiTinh.Value;
            else
                cmd.Parameters.Add("@GioiTinh", SqlDbType.TinyInt).Value = DBNull.Value;

            cmd.Parameters.AddWithValue("@DienThoai", (object?)kh.DienThoai ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", (object?)kh.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DiaChi", (object?)kh.DiaChi ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GhiChu", (object?)kh.GhiChu ?? DBNull.Value);

            return cmd.ExecuteNonQuery();
        }

        public int Delete(int khachHangId)
        {
            using var conn = Db.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"DELETE FROM DM_KhachHang WHERE KhachHangId = @KhachHangId;";
            cmd.Parameters.AddWithValue("@KhachHangId", khachHangId);

            return cmd.ExecuteNonQuery();
        }

        // ====== HÀM TỰ SINH MÃ KHÁCH HÀNG: KH001, KH002, ... ======
        private string GenerateMaKhachHang(SqlConnection conn)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
SELECT ISNULL(MAX(CAST(SUBSTRING(MaKhachHang, 3, 10) AS INT)), 0)
FROM DM_KhachHang
WHERE MaKhachHang LIKE 'KH%' AND ISNUMERIC(SUBSTRING(MaKhachHang, 3, 10)) = 1;
";

            var obj = cmd.ExecuteScalar();
            var max = 5;
            if (obj != null && obj != DBNull.Value)
            {
                int.TryParse(obj.ToString(), out max);
            }

            var next = max + 1;              // KH005 -> 5 -> 6
            return "KH" + next.ToString("D3");   // => KH006
        }
    }
}
