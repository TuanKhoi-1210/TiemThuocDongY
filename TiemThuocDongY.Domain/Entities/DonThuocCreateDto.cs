using System.Collections.Generic;

namespace TiemThuocDongY.Domain.Entities
{
    public class DonThuocCreateDto
    {
        public DonThuoc Header { get; set; }
        public List<DonThuocChiTiet> Details { get; set; }
    }
}
