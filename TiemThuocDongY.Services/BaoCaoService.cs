using TiemThuocDongY.Data;
using TiemThuocDongY.Domain.Entities;

namespace TiemThuocDongY.Services
{
    public class BaoCaoService
    {
        private readonly BaoCaoRepository _repo;

        public BaoCaoService(BaoCaoRepository repo)
        {
            _repo = repo;
        }

        public BaoCaoTongHopThangDto GetTongHopThang(int year, int month)
        {
            return _repo.GetTongHopThang(year, month);
        }
    }
}
