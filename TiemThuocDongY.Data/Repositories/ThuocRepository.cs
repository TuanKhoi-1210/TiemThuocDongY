using System;
using System.Collections.Generic;
using System.Data;
using TiemThuocDongY.Data.Infrastructure;
using TiemThuocDongY.Domain.Entities;

public class ThuocRepository
{
    public IList<Thuoc> GetAll()
    {
        var items = new List<Thuoc>();

        using var conn = Db.GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = @"
SELECT ThuocId, MaThuoc, TenThuoc, TenKhac, DonViTinh,
       GiaBanLe, TonToiThieu, SoLuongTon, CongDung, ChongChiDinh, GhiChu, IsActive
FROM DM_Thuoc
WHERE IsActive = 1
ORDER BY MaThuoc;
";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var t = new Thuoc
            {
                ThuocId = reader.GetInt32(reader.GetOrdinal("ThuocId")),
                MaThuoc = reader.GetString(reader.GetOrdinal("MaThuoc")),
                TenThuoc = reader.GetString(reader.GetOrdinal("TenThuoc")),
                TenKhac = reader["TenKhac"] as string,
                DonViTinh = reader.GetString(reader.GetOrdinal("DonViTinh")),
                GiaBanLe = reader.GetDecimal(reader.GetOrdinal("GiaBanLe")),
                TonToiThieu = reader.GetDecimal(reader.GetOrdinal("TonToiThieu")),
                SoLuongTon = reader.GetDecimal(reader.GetOrdinal("SoLuongTon")),
                CongDung = reader["CongDung"] as string,
                ChongChiDinh = reader["ChongChiDinh"] as string,
                GhiChu = reader["GhiChu"] as string,
                IsActive = (bool)reader["IsActive"]
            };
            items.Add(t);
        }

        return items;
    }

    // Trả về int: Id vừa insert
    public int Insert(Thuoc thuoc)
    {
        using var conn = Db.GetConnection();
        conn.Open();

        // 🔒 LUÔN tự sinh mã thuốc (T001, T002, ...)
        thuoc.MaThuoc = GenerateMaThuoc(conn);

        using var cmd = conn.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = @"
INSERT INTO DM_Thuoc
(MaThuoc, TenThuoc, TenKhac, DonViTinh, GiaBanLe, TonToiThieu,
 CongDung, ChongChiDinh, GhiChu, IsActive, CreatedDate)
VALUES
(@MaThuoc, @TenThuoc, @TenKhac, @DonViTinh, @GiaBanLe, @TonToiThieu,
 @CongDung, @ChongChiDinh, @GhiChu, 1, SYSDATETIME());

SELECT SCOPE_IDENTITY();
";

        cmd.Parameters.AddWithValue("@MaThuoc", thuoc.MaThuoc);
        cmd.Parameters.AddWithValue("@TenThuoc", thuoc.TenThuoc);
        cmd.Parameters.AddWithValue("@TenKhac", (object?)thuoc.TenKhac ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DonViTinh", thuoc.DonViTinh);
        cmd.Parameters.AddWithValue("@GiaBanLe", thuoc.GiaBanLe);
        cmd.Parameters.AddWithValue("@TonToiThieu", thuoc.TonToiThieu);
        cmd.Parameters.AddWithValue("@CongDung", (object?)thuoc.CongDung ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ChongChiDinh", (object?)thuoc.ChongChiDinh ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@GhiChu", (object?)thuoc.GhiChu ?? DBNull.Value);

        var idObj = cmd.ExecuteScalar();
        var id = Convert.ToInt32(idObj);
        return id;
    }

    // Trả về số dòng update (0 = không có dòng nào đổi)
    // 🔒 Không cho sửa MaThuoc nữa
    public int Update(Thuoc thuoc)
    {
        using var conn = Db.GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = @"
UPDATE DM_Thuoc
SET TenThuoc     = @TenThuoc,
    TenKhac      = @TenKhac,
    DonViTinh    = @DonViTinh,
    GiaBanLe     = @GiaBanLe,
    TonToiThieu  = @TonToiThieu,
    CongDung     = @CongDung,
    ChongChiDinh = @ChongChiDinh,
    GhiChu       = @GhiChu
WHERE ThuocId = @ThuocId;
";

        cmd.Parameters.AddWithValue("@ThuocId", thuoc.ThuocId);
        // KHÔNG còn @MaThuoc trong câu lệnh Update
        cmd.Parameters.AddWithValue("@TenThuoc", thuoc.TenThuoc);
        cmd.Parameters.AddWithValue("@TenKhac", (object?)thuoc.TenKhac ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DonViTinh", thuoc.DonViTinh);
        cmd.Parameters.AddWithValue("@GiaBanLe", thuoc.GiaBanLe);
        cmd.Parameters.AddWithValue("@TonToiThieu", thuoc.TonToiThieu);
        cmd.Parameters.AddWithValue("@CongDung", (object?)thuoc.CongDung ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ChongChiDinh", (object?)thuoc.ChongChiDinh ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@GhiChu", (object?)thuoc.GhiChu ?? DBNull.Value);

        var rows = cmd.ExecuteNonQuery();
        return rows;
    }

    public void SoftDelete(int thuocId)
    {
        using var conn = Db.GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = @"
UPDATE DM_Thuoc
SET IsActive = 0
WHERE ThuocId = @ThuocId;
";
        cmd.Parameters.AddWithValue("@ThuocId", thuocId);
        cmd.ExecuteNonQuery();
    }

    // ====== HÀM TỰ SINH MÃ THUỐC: T001, T002, ... ======
    private string GenerateMaThuoc(IDbConnection conn)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = @"
SELECT ISNULL(MAX(CAST(SUBSTRING(MaThuoc, 2, 10) AS INT)), 0)
FROM DM_Thuoc
WHERE MaThuoc LIKE 'T%' AND ISNUMERIC(SUBSTRING(MaThuoc, 2, 10)) = 1;
";

        var obj = cmd.ExecuteScalar();
        var max = 10;
        if (obj != null && obj != DBNull.Value)
        {
            int.TryParse(obj.ToString(), out max);
        }

        var next = max + 1;      // nếu đang tới T010 -> max = 10 -> next = 11
        return "T" + next.ToString("D3"); // => T011
    }
}
