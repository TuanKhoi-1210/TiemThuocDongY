using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TiemThuocDongY.Data.Infrastructure;
using TiemThuocDongY.Domain.Entities;

namespace TiemThuocDongY.Data.Repositories
{
    public class UserRepository
    {
        // =============== LẤY 1 USER THEO USERNAME (DÙNG CHO LOGIN) ===============
        public SysUser GetByUserName(string userName)
        {
            using (var conn = Db.GetConnection())
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
SELECT TOP 1 u.UserId, u.UserName, u.PasswordHash, u.FullName,
       u.Email, u.PhoneNumber, u.IsEmailConfirmed, u.IsActive,
       u.RoleId, r.RoleName
FROM Sys_User u
JOIN Sys_Role r ON u.RoleId = r.RoleId
WHERE u.UserName = @UserName;
";
                    cmd.Parameters.AddWithValue("@UserName", userName);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            return null;

                        return MapUser(reader);
                    }
                }
            }
        }
        public SysUser? GetByUserNameOrEmail(string userNameOrEmail)
        {
            using var conn = Db.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
SELECT TOP 1 u.UserId, u.UserName, u.PasswordHash, u.FullName,
       u.Email, u.PhoneNumber, u.IsEmailConfirmed, u.IsActive,
       u.RoleId, r.RoleName
FROM Sys_User u
JOIN Sys_Role r ON u.RoleId = r.RoleId
WHERE (u.UserName = @Id OR u.Email = @Id) AND u.IsActive = 1;";

            cmd.Parameters.AddWithValue("@Id", userNameOrEmail);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            return new SysUser
            {
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                UserName = reader.GetString(reader.GetOrdinal("UserName")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                PhoneNumber = reader["PhoneNumber"] as string,
                IsEmailConfirmed = (bool)reader["IsEmailConfirmed"],
                IsActive = (bool)reader["IsActive"],
                RoleId = reader.GetInt32(reader.GetOrdinal("RoleId")),
                RoleName = reader.GetString(reader.GetOrdinal("RoleName"))
            };
        }
        // =============== LẤY TẤT CẢ USER KÈM ROLE (CHO MÀN HÌNH QUẢN LÝ TK) ===============
        public IEnumerable<SysUser> GetAllWithRoleName()
        {
            var list = new List<SysUser>();

            using (var conn = Db.GetConnection())
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
SELECT u.UserId, u.UserName, u.PasswordHash, u.FullName,
       u.Email, u.PhoneNumber, u.IsEmailConfirmed, u.IsActive,
       u.RoleId, r.RoleName
FROM Sys_User u
JOIN Sys_Role r ON u.RoleId = r.RoleId
ORDER BY u.UserName;
";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(MapUser(reader));
                        }
                    }
                }
            }

            return list;
        }

        // =============== THÊM TÀI KHOẢN MỚI ===============
        public int Insert(SysUser user)
        {
            using (var conn = Db.GetConnection())
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
INSERT INTO Sys_User
    (UserName, PasswordHash, FullName, Email,
     IsActive, RoleId, CreatedDate, PhoneNumber, IsEmailConfirmed)
VALUES
    (@UserName, @PasswordHash, @FullName, @Email,
     @IsActive, @RoleId, SYSDATETIME(), @PhoneNumber, @IsEmailConfirmed);

SELECT SCOPE_IDENTITY();
";

                    cmd.Parameters.AddWithValue("@UserName", user.UserName);
                    cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    cmd.Parameters.AddWithValue("@FullName", user.FullName);
                    cmd.Parameters.AddWithValue("@Email", (object)user.Email ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
                    cmd.Parameters.AddWithValue("@RoleId", user.RoleId);
                    cmd.Parameters.AddWithValue("@PhoneNumber", (object)user.PhoneNumber ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsEmailConfirmed", user.IsEmailConfirmed);

                    var obj = cmd.ExecuteScalar();
                    var id = Convert.ToInt32(obj);
                    user.UserId = id;
                    return id;
                }
            }
        }

        // =============== SỬA HỌ TÊN / EMAIL / SĐT / VAI TRÒ ===============
        public void UpdateBasicInfoAndRole(
            int userId,
            string fullName,
            string email,
            string phoneNumber,
            int roleId)
        {
            using (var conn = Db.GetConnection())
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
UPDATE Sys_User
SET FullName   = @FullName,
    Email      = @Email,
    PhoneNumber = @PhoneNumber,
    RoleId     = @RoleId
WHERE UserId   = @UserId;
";
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@FullName", fullName);
                    cmd.Parameters.AddWithValue("@Email", (object)email ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PhoneNumber", (object)phoneNumber ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RoleId", roleId);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // =============== ĐỔI MẬT KHẨU ===============
        public void UpdatePassword(int userId, string passwordHash)
        {
            using var conn = Db.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "UPDATE Sys_User SET PasswordHash = @Pwd WHERE UserId = @UserId;";
            cmd.Parameters.AddWithValue("@Pwd", passwordHash);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.ExecuteNonQuery();
        }

        // =============== KHÓA / MỞ TÀI KHOẢN ===============
        public void SetActive(int userId, bool isActive)
        {
            using (var conn = Db.GetConnection())
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
UPDATE Sys_User
SET IsActive = @IsActive
WHERE UserId = @UserId;
";
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@IsActive", isActive);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // =============== XÓA TÀI KHOẢN ===============
        public void Delete(int userId)
        {
            using (var conn = Db.GetConnection())
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "DELETE FROM Sys_User WHERE UserId = @UserId;";
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // =============== HÀM MAP DATAREADER -> SysUser ===============
        private static SysUser MapUser(SqlDataReader reader)
        {
            var user = new SysUser();

            user.UserId = reader.GetInt32(reader.GetOrdinal("UserId"));
            user.UserName = reader.GetString(reader.GetOrdinal("UserName"));
            user.PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash"));
            user.FullName = reader.GetString(reader.GetOrdinal("FullName"));
            user.Email = reader["Email"] == DBNull.Value ? null : (string)reader["Email"];
            user.PhoneNumber = reader["PhoneNumber"] == DBNull.Value ? null : (string)reader["PhoneNumber"];
            user.IsEmailConfirmed = (bool)reader["IsEmailConfirmed"];
            user.IsActive = (bool)reader["IsActive"];
            user.RoleId = reader.GetInt32(reader.GetOrdinal("RoleId"));
            user.RoleName = reader.GetString(reader.GetOrdinal("RoleName"));

            return user;
        }
    }
}
