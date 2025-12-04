using System;
using System.Collections.Generic;

namespace TiemThuocDongY.Domain.Entities
{
    public class PhieuNhapListItem
    {
        public int PhieuNhapId { get; set; }
        public string SoPhieu { get; set; } = "";
        public DateTime NgayNhap { get; set; }

        public int NhaCungCapId { get; set; }
        public string MaNcc { get; set; } = "";
        public string TenNcc { get; set; } = "";

        public int SoMatHang { get; set; }
        public decimal TongSoLuong { get; set; }

        public decimal TongTienHang { get; set; }
        public decimal GiamGia { get; set; }
        public decimal TienPhaiTra { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal ConNo { get; set; }

        public DateTime? HanThanhToan { get; set; }
        public byte TrangThaiThanhToan { get; set; }
    }

    public class PhieuNhapDetailLineDto
    {
        public int PhieuNhapChiTietId { get; set; }
        public int ThuocId { get; set; }
        public string TenThuoc { get; set; } = "";
        public string DonViTinh { get; set; } = "";
        public decimal SoLuong { get; set; }
        public decimal DonGiaNhap { get; set; }
        public decimal ThanhTien { get; set; }
        public string GhiChu { get; set; } = "";
    }

    public class PhieuNhapDetailDto
    {
        public int PhieuNhapId { get; set; }
        public string SoPhieu { get; set; } = "";
        public DateTime NgayNhap { get; set; }

        public int NhaCungCapId { get; set; }
        public string MaNcc { get; set; } = "";
        public string TenNcc { get; set; } = "";
        public string NguoiLienHe { get; set; } = "";
        public string DiaChi { get; set; } = "";

        public decimal TongTienHang { get; set; }
        public decimal GiamGia { get; set; }
        public decimal TienPhaiTra { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal ConNo { get; set; }

        public DateTime? HanThanhToan { get; set; }
        public byte TrangThaiThanhToan { get; set; }
        public string GhiChu { get; set; } = "";

        public List<PhieuNhapDetailLineDto> ChiTiet { get; set; } = new();
    }

    // DTO nhận từ JS khi tạo phiếu nhập
    public class PhieuNhapCreateHeaderDto
    {
        public int PhieuNhapId { get; set; }
        public int NhaCungCapId { get; set; }
        public string TenNhaCungCap { get; set; } = "";
        public string SoPhieu { get; set; } = "";
        public DateTime NgayNhap { get; set; }
        public decimal TongTienHang { get; set; }
        public decimal GiamGia { get; set; }
        public DateTime? HanThanhToan { get; set; }
        public string GhiChu { get; set; } = "";
    }

    public class PhieuNhapCreateLineDto
    {
        public int ThuocId { get; set; }
        public decimal SoLuong { get; set; }
        public decimal DonGiaNhap { get; set; }
        public decimal ThanhTien { get; set; }
        public string GhiChu { get; set; } = "";
    }

    public class PhieuNhapCreateDto
    {
        public PhieuNhapCreateHeaderDto Header { get; set; } = new();
        public List<PhieuNhapCreateLineDto> Details { get; set; } = new();
    }
}
