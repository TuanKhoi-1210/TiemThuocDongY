using System;
using System.Collections.Generic;

namespace TiemThuocDongY.Domain.Entities
{
    public class BaoCao7NgayItem
    {
        public DateTime Ngay { get; set; }
        public int SoDon { get; set; }
        public decimal DoanhThu { get; set; }
    }

    public class BaoCaoTopThuocItem
    {
        public int ThuocId { get; set; }
        public string TenThuoc { get; set; } = "";
        public string DonViTinh { get; set; } = "";
        public decimal SoLuongBan { get; set; }
        public decimal DoanhThu { get; set; }
    }

    public class BaoCaoCongNoKhachHangItem
    {
        public int KhachHangId { get; set; }
        public string MaKhachHang { get; set; } = "";
        public string HoTen { get; set; } = "";
        public decimal TongNo { get; set; }
        public int SoDonConNo { get; set; }
        public DateTime? HanThanhToanGanNhat { get; set; }
    }

    public class BaoCaoCongNoNhaCungCapItem
    {
        public int NhaCungCapId { get; set; }
        public string MaNCC { get; set; } = "";
        public string TenNCC { get; set; } = "";
        public decimal TongNo { get; set; }
        public int SoPhieuConNo { get; set; }
        public DateTime? HanThanhToanGanNhat { get; set; }
    }

    public class BaoCaoTongHopThangDto
    {
        public int Year { get; set; }
        public int Month { get; set; }

        public decimal DoanhThuHomNay { get; set; }
        public int SoDonHomNay { get; set; }

        public decimal DoanhThuThang { get; set; }
        public int SoDonThang { get; set; }
        public decimal DoanhThuThangTruoc { get; set; }

        public decimal GiaTriNhapThang { get; set; }
        public int SoPhieuNhapThang { get; set; }

        public List<BaoCao7NgayItem> DoanhThu7Ngay { get; set; } = new();
        public List<BaoCaoTopThuocItem> TopThuocThang { get; set; } = new();

        public decimal TongPhaiThuKhachHang { get; set; }
        public int SoKhachConNo { get; set; }

        public decimal TongPhaiTraNhaCungCap { get; set; }
        public int SoNccConNo { get; set; }

        public List<BaoCaoCongNoKhachHangItem> CongNoKhachHang { get; set; } = new();
        public List<BaoCaoCongNoNhaCungCapItem> CongNoNhaCungCap { get; set; } = new();
    }
}
