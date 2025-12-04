namespace TiemThuocDongY.Domain.Entities
{
    public class DonThuocChiTiet
    {
        public int DonThuocChiTietId { get; set; }
        public int DonThuocId { get; set; }
        public int ThuocId { get; set; }
        public decimal LieuLuongGram { get; set; }
        public int SoThang { get; set; }
        public decimal DonGiaBan { get; set; }
        public decimal ThanhTien { get; set; }
        public string GhiChu { get; set; }
    }
}
