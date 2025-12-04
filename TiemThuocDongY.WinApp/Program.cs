using System;
using System.Windows.Forms;
using TiemThuocDongY.Data.Infrastructure;
using QuestPDF.Infrastructure;

namespace TiemThuocDongY.WinApp;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        // TODO: sửa chuỗi kết nối cho đúng SQL Server của em
        Db.ConnectionString = "Server=localhost\\SQLEXPRESS;Database=QL_TiemThuocDongY;Trusted_Connection=True;TrustServerCertificate=True;";
        QuestPDF.Settings.License = LicenseType.Community;
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
