using System.Data.SqlClient;

namespace TiemThuocDongY.Data.Infrastructure;

public static class Db
{
    /// <summary>
    /// Được set từ WinApp.Program khi khởi động.
    /// </summary>
    public static string ConnectionString { get; set; } = string.Empty;

    public static SqlConnection GetConnection()
        => new SqlConnection(ConnectionString);
}
