using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TiemThuocDongY.Data.Infrastructure;
using TiemThuocDongY.Domain.Entities;

namespace TiemThuocDongY.Data.Repositories
{
    public class NhaCungCapRepository
    {
        public IList<DM_NhaCungCap> GetAll()
        {
            var list = new List<DM_NhaCungCap>();

            using var conn = Db.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
SELECT  NhaCungCapId,
        MaNCC,
        TenNCC,
        NguoiLienHe,
        DienThoai,
        Email,
        DiaChi,
        GhiChu
FROM    DM_NhaCungCap
ORDER BY TenNCC;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var ncc = new DM_NhaCungCap
                {
                    NhaCungCapId = reader.GetInt32(reader.GetOrdinal("NhaCungCapId")),
                    MaNcc = reader["MaNCC"]?.ToString() ?? "",
                    TenNcc = reader["TenNCC"]?.ToString() ?? "",
                    NguoiLienHe = reader["NguoiLienHe"]?.ToString() ?? "",
                    DienThoai = reader["DienThoai"]?.ToString() ?? "",
                    Email = reader["Email"]?.ToString() ?? "",
                    DiaChi = reader["DiaChi"]?.ToString() ?? "",
                    GhiChu = reader["GhiChu"]?.ToString() ?? ""
                };

                list.Add(ncc);
            }

            return list;
        }

        public int Insert(DM_NhaCungCap ncc, int createdBy)
        {
            using var conn = Db.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO DM_NhaCungCap
(MaNCC, TenNCC, NguoiLienHe, DienThoai, Email, DiaChi, GhiChu, CreatedDate, CreatedBy)
OUTPUT INSERTED.NhaCungCapId
VALUES
(@Ma, @Ten, @NguoiLienHe, @DienThoai, @Email, @DiaChi, @GhiChu, GETDATE(), @CreatedBy);";

            cmd.Parameters.AddWithValue("@Ma", ncc.MaNcc ?? "");
            cmd.Parameters.AddWithValue("@Ten", ncc.TenNcc ?? "");
            cmd.Parameters.AddWithValue("@NguoiLienHe", ncc.NguoiLienHe ?? "");
            cmd.Parameters.AddWithValue("@DienThoai", ncc.DienThoai ?? "");
            cmd.Parameters.AddWithValue("@Email", ncc.Email ?? "");
            cmd.Parameters.AddWithValue("@DiaChi", ncc.DiaChi ?? "");
            cmd.Parameters.AddWithValue("@GhiChu", ncc.GhiChu ?? "");
            cmd.Parameters.AddWithValue("@CreatedBy", createdBy);

            int newId = Convert.ToInt32(cmd.ExecuteScalar());
            ncc.NhaCungCapId = newId;
            return newId;
        }

        public void Update(DM_NhaCungCap ncc)
        {
            using var conn = Db.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
UPDATE DM_NhaCungCap SET
    MaNCC       = @Ma,
    TenNCC      = @Ten,
    NguoiLienHe = @NguoiLienHe,
    DienThoai   = @DienThoai,
    Email       = @Email,
    DiaChi      = @DiaChi,
    GhiChu      = @GhiChu
WHERE NhaCungCapId = @Id;";

            cmd.Parameters.AddWithValue("@Id", ncc.NhaCungCapId);
            cmd.Parameters.AddWithValue("@Ma", ncc.MaNcc ?? "");
            cmd.Parameters.AddWithValue("@Ten", ncc.TenNcc ?? "");
            cmd.Parameters.AddWithValue("@NguoiLienHe", ncc.NguoiLienHe ?? "");
            cmd.Parameters.AddWithValue("@DienThoai", ncc.DienThoai ?? "");
            cmd.Parameters.AddWithValue("@Email", ncc.Email ?? "");
            cmd.Parameters.AddWithValue("@DiaChi", ncc.DiaChi ?? "");
            cmd.Parameters.AddWithValue("@GhiChu", ncc.GhiChu ?? "");

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = Db.GetConnection();
            conn.Open();

            // Không cho xóa nếu đã có phiếu nhập
            using (var cmdCheck = conn.CreateCommand())
            {
                cmdCheck.CommandText = "SELECT COUNT(*) FROM PhieuNhap WHERE NhaCungCapId = @Id";
                cmdCheck.Parameters.AddWithValue("@Id", id);
                int count = (int)cmdCheck.ExecuteScalar();
                if (count > 0)
                    throw new Exception("Nhà cung cấp này đã có phiếu nhập, không thể xóa.");
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM DM_NhaCungCap WHERE NhaCungCapId = @Id";
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
