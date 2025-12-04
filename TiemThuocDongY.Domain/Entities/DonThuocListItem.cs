using System;

namespace TiemThuocDongY.Domain.Entities
{
    public class DonThuocListItem
    {
        public int DonThuocId { get; set; }
        public string SoDon { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; }

        public int KhachHangId { get; set; }
        public string MaKhachHang { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;

        public string BacSiKeDon { get; set; } = string.Empty;

        public decimal TongTienHang { get; set; }
        public decimal GiamGia { get; set; }
        public decimal TienKhachPhaiTra { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal ConNo { get; set; }

        public DateTime? HanThanhToan { get; set; }

        public byte TrangThaiDon { get; set; }
        public byte TrangThaiThanhToan { get; set; }

        public string GhiChu { get; set; } = string.Empty;
    }
}
