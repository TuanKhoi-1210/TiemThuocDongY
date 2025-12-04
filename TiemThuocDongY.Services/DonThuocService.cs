using System;
using System.Collections.Generic;
using TiemThuocDongY.Data.Repositories;
using TiemThuocDongY.Domain.Entities;

namespace TiemThuocDongY.Services
{
    public class DonThuocService
    {
        private readonly DonThuocRepository _repo = new DonThuocRepository();

        public int Create(DonThuoc h, List<DonThuocChiTiet> d)
        {
            h.SoDon = _repo.GenerateSoDon();

            h.TrangThaiDon = 0;

            // Hạn thanh toán mặc định 15 ngày từ ngày lập (nếu chưa có)
            if (!h.HanThanhToan.HasValue)
            {
                h.HanThanhToan = h.NgayLap.AddDays(15);
            }

            // Trạng thái đơn: khi mới tạo coi như "Đã kê đơn"
            if (h.TrangThaiDon == 0)
            {
                h.TrangThaiDon = 1; // 1 = Đã kê đơn (theo mapping của anh)
            }

            // TÍNH TRẠNG THÁI THANH TOÁN
            ApplyPaymentStatus(h);

            return _repo.Insert(h, d);
        }

        public void Update(DonThuoc h, List<DonThuocChiTiet> d)
        {
            // Không cho sửa ngày lập nếu anh đã khóa bên UI – nhưng ở đây cứ tin dữ liệu gửi lên

            // Không tự động đổi hạn thanh toán nếu anh muốn giữ DB cũ
            // Nếu muốn auto chỉnh lại khi sửa, có thể thêm:
            // if (!h.HanThanhToan.HasValue) h.HanThanhToan = h.NgayLap.AddDays(15);

            // TÍNH LẠI TRẠNG THÁI THANH TOÁN MỖI LẦN LƯU
            ApplyPaymentStatus(h);

            _repo.Update(h, d);
        }
        private void ApplyPaymentStatus(DonThuoc h)
        {
            // Nếu TienKhachPhaiTra chưa set từ UI thì tự tính = Tổng - Giảm
            var phaiTra = h.TienKhachPhaiTra;
            if (phaiTra <= 0)
            {
                phaiTra = h.TongTienHang - h.GiamGia;
                if (phaiTra < 0) phaiTra = 0;
                h.TienKhachPhaiTra = phaiTra;
            }

            var daTra = h.DaThanhToan;

            if (phaiTra <= 0)
            {
                // Không thu tiền
                h.TrangThaiThanhToan = 3;   // "Không thu"
                h.ConNo = 0;
                return;
            }

            if (daTra <= 0)
            {
                h.TrangThaiThanhToan = 0;   // "Chưa thanh toán"
                h.ConNo = phaiTra;
            }
            else if (daTra >= phaiTra)
            {
                h.TrangThaiThanhToan = 2;   // "Đã thanh toán"
                h.ConNo = 0;
            }
            else
            {
                h.TrangThaiThanhToan = 1;   // "Trả một phần"
                h.ConNo = phaiTra - daTra;
            }
        }
        public void MarkAsCompleted(int id)
        {
            _repo.MarkAsCompleted(id);
        }

        public DonThuocDetailDto GetDetail(int id)
        {
            return _repo.GetDetail(id);
        }
        public void Delete(int id)
        {
            _repo.Delete(id);
        }

    }

}