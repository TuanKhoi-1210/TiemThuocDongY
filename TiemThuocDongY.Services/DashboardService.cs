using System;
using TiemThuocDongY.Data;
using TiemThuocDongY.Domain.Entities;

namespace TiemThuocDongY.Services
{
    public class DashboardService
    {
        private readonly DashboardRepository _repo;

        public DashboardService(DashboardRepository repo)
        {
            _repo = repo;
        }

        public DashboardSummaryDto GetTodaySummary()
        {
            return _repo.GetSummary(DateTime.Today);
        }
    }
}
