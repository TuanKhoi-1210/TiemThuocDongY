using System;
using System.Data;
using TiemThuocDongY.Data.Infrastructure;
using TiemThuocDongY.Domain.Entities;

namespace TiemThuocDongY.Data.Repositories
{
    public class PasswordResetRepository
    {
        public int Insert(int userId, string code, DateTime expiresAt)
        {
            using var conn = Db.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
INSERT INTO Sys_PasswordReset(UserId, Code, ExpiresAt, IsUsed)
VALUES (@UserId, @Code, @ExpiresAt, 0);
SELECT SCOPE_IDENTITY();";

            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@Code", code);
            cmd.Parameters.AddWithValue("@ExpiresAt", expiresAt);

            var idObj = cmd.ExecuteScalar();
            return Convert.ToInt32(idObj);
        }

        public PasswordReset? GetValidByUserAndCode(int userId, string code)
        {
            using var conn = Db.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
SELECT TOP 1 ResetId, UserId, Code, ExpiresAt, IsUsed
FROM Sys_PasswordReset
WHERE UserId = @UserId
  AND Code = @Code
  AND IsUsed = 0
  AND ExpiresAt >= GETDATE()
ORDER BY ResetId DESC;";

            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@Code", code);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            return new PasswordReset
            {
                ResetId = reader.GetInt32(reader.GetOrdinal("ResetId")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                Code = reader.GetString(reader.GetOrdinal("Code")),
                ExpiresAt = reader.GetDateTime(reader.GetOrdinal("ExpiresAt")),
                IsUsed = reader.GetBoolean(reader.GetOrdinal("IsUsed"))
            };
        }

        public void MarkUsed(int resetId)
        {
            using var conn = Db.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "UPDATE Sys_PasswordReset SET IsUsed = 1 WHERE ResetId = @Id;";
            cmd.Parameters.AddWithValue("@Id", resetId);
            cmd.ExecuteNonQuery();
        }
    }
}
