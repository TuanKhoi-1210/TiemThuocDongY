using System;

namespace TiemThuocDongY.Domain.Entities
{
    public class DonThuoc
    {
        public int DonThuocId { get; set; }
        public string SoDon { get; set; }
        public DateTime NgayLap { get; set; }
        public int KhachHangId { get; set; }
        public string BacSiKeDon { get; set; }
        public string ChanDoan { get; set; }
        public decimal TongTienHang { get; set; }
        public decimal GiamGia { get; set; }
        public decimal TienKhachPhaiTra { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal ConNo { get; set; }
        public DateTime? HanThanhToan { get; set; }
        public byte TrangThaiDon { get; set; } = 0;
        public byte TrangThaiThanhToan { get; set; } = 0;
        public string GhiChu { get; set; }
        public int CreatedBy { get; set; }
    }
}
