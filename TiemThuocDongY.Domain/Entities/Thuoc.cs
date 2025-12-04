namespace TiemThuocDongY.Domain.Entities;

public class Thuoc
{
    public int ThuocId { get; set; }
    public string MaThuoc { get; set; } = string.Empty;
    public string TenThuoc { get; set; } = string.Empty;
    public string? TenKhac { get; set; }
    public string DonViTinh { get; set; } = string.Empty;
    public decimal GiaBanLe { get; set; }
    public decimal TonToiThieu { get; set; }

    public decimal SoLuongTon { get; set; }
    public string? CongDung { get; set; }
    public string? ChongChiDinh { get; set; }
    public string? GhiChu { get; set; }
    public bool IsActive { get; set; }
}
