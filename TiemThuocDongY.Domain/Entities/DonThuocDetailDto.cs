using System;
using System.Collections.Generic;

namespace TiemThuocDongY.Domain.Entities
{
    public class DonThuocDetailDto
    {
        public int DonThuocId { get; set; }
        public string SoDon { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; }
        public string BacSiKeDon { get; set; } = string.Empty;
        public string ChanDoan { get; set; } = string.Empty;
        public decimal TongTienHang { get; set; }
        public decimal GiamGia { get; set; }
        public decimal TienKhachPhaiTra { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal ConNo { get; set; }
        public DateTime? HanThanhToan { get; set; }
        public byte TrangThaiDon { get; set; }
        public byte TrangThaiThanhToan { get; set; }
        public string GhiChu { get; set; } = string.Empty;

        public int KhachHangId { get; set; }
        public string MaKhachHang { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;
        public int? NamSinh { get; set; }
        public string DiaChi { get; set; } = string.Empty;

        public List<DonThuocDetailLineDto> ChiTiet { get; set; } = new();
    }

    public class DonThuocDetailLineDto
    {
        public int DonThuocChiTietId { get; set; }
        public int ThuocId { get; set; }
        public string TenThuoc { get; set; } = string.Empty;
        public decimal LieuLuongGram { get; set; }
        public int SoThang { get; set; }
        public decimal DonGiaBan { get; set; }
        public decimal ThanhTien { get; set; }
        public string GhiChu { get; set; } = string.Empty;
    }
}
