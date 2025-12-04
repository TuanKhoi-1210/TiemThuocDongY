using System;

namespace TiemThuocDongY.Domain.Entities
{
    public class DM_KhachHang
    {
        public int KhachHangId { get; set; }
        public string MaKhachHang { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;

        public int? NamSinh { get; set; }

        // map với cột [GioiTinh] TINYINT NULL trong DM_KhachHang
        // 1 = Nam, 2 = Nữ, 3 = Khác (tùy bạn quy ước)
        public byte? GioiTinh { get; set; }

        public string DienThoai { get; set; } = string.Empty;

        // map với cột [Email] NVARCHAR(100) NULL
        public string Email { get; set; } = string.Empty;

        public string DiaChi { get; set; } = string.Empty;
        public string GhiChu { get; set; } = string.Empty;
    }

}
