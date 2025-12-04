using System;
using System.Globalization;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TiemThuocDongY.Domain.Entities;

namespace TiemThuocDongY.WinApp.Printing
{
    public class PhieuNhapPdfDocument : IDocument
    {
        private readonly PhieuNhapDetailDto _model;
        private static readonly CultureInfo ViCulture = new("vi-VN");

        public PhieuNhapPdfDocument(PhieuNhapDetailDto model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        // ====== IDocument implementation ======

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Times New Roman"));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Trang ");
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        }

        // ================= HEADER =================
        void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                // Dòng trên cùng: thông tin tiệm + số phiếu / ngày
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("TIỆM THUỐC ĐÔNG Y")
                            .SemiBold().FontSize(14);
                        col.Item().Text("Địa chỉ: ........................................");
                        col.Item().Text("Điện thoại: .................................");
                    });

                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text($"Số phiếu: {_model.SoPhieu}")
                            .SemiBold();
                        col.Item().Text($"Ngày nhập: {_model.NgayNhap:dd/MM/yyyy}");
                    });
                });

                // Tiêu đề
                column.Item().PaddingTop(15).AlignCenter().Text(text =>
                {
                    text.Span("PHIẾU NHẬP HÀNG")
                        .Bold().FontSize(18).Underline();
                });

                // Thông tin nhà cung cấp
                column.Item().PaddingTop(10).Text(txt =>
                {
                    txt.Span("Nhà cung cấp: ").SemiBold();
                    txt.Span($"{_model.TenNcc} ({_model.MaNcc})");
                });

                column.Item().Text(txt =>
                {
                    txt.Span("Người liên hệ: ").SemiBold();
                    txt.Span(_model.NguoiLienHe ?? "");
                });

                column.Item().Text(txt =>
                {
                    txt.Span("Địa chỉ: ").SemiBold();
                    txt.Span(_model.DiaChi ?? "");
                });
            });
        }

        // ================= CONTENT =================
        void ComposeContent(IContainer container)
        {
            var phaiTra = _model.TienPhaiTra;
            if (phaiTra <= 0)
                phaiTra = _model.TongTienHang - _model.GiamGia;

            var conNo = _model.ConNo;

            container.PaddingTop(15).Column(column =>
            {
                // ---- BẢNG HÀNG NHẬP ----
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(25);     // STT
                        columns.RelativeColumn(3);      // Tên thuốc
                        columns.RelativeColumn(1.2f);   // ĐVT
                        columns.RelativeColumn(1.2f);   // Số lượng
                        columns.RelativeColumn(1.5f);   // Đơn giá
                        columns.RelativeColumn(1.5f);   // Thành tiền
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("STT");
                        header.Cell().Element(HeaderCell).Text("Tên thuốc");
                        header.Cell().Element(HeaderCell).Text("ĐVT");
                        header.Cell().Element(HeaderCell).Text("Số lượng");
                        header.Cell().Element(HeaderCell).Text("Đơn giá nhập");
                        header.Cell().Element(HeaderCell).Text("Thành tiền");

                        static IContainer HeaderCell(IContainer container) =>
                            container.DefaultTextStyle(x => x.SemiBold())
                                     .Padding(4)
                                     .Background(Colors.Grey.Lighten3)
                                     .BorderBottom(1)
                                     .BorderColor(Colors.Grey.Darken2);
                    });

                    // Rows
                    var lines = _model.ChiTiet ?? Enumerable.Empty<PhieuNhapDetailLineDto>();

                    int index = 1;
                    foreach (var line in lines)
                    {
                        table.Cell().Element(Cell).AlignCenter().Text(index.ToString());
                        table.Cell().Element(Cell).Text(line.TenThuoc);
                        table.Cell().Element(Cell).AlignCenter().Text(line.DonViTinh);
                        table.Cell().Element(Cell).AlignRight()
                            .Text(line.SoLuong.ToString("0.##", ViCulture));
                        table.Cell().Element(Cell).AlignRight()
                            .Text(FormatMoney(line.DonGiaNhap));
                        table.Cell().Element(Cell).AlignRight()
                            .Text(FormatMoney(line.ThanhTien));

                        index++;
                    }

                    static IContainer Cell(IContainer container) =>
                        container.Padding(4)
                                 .BorderBottom(0.5f)
                                 .BorderColor(Colors.Grey.Lighten2);
                });

                // ---- TỔNG TIỀN / GIẢM GIÁ / CÔN NỢ ----
                column.Item().PaddingTop(10).AlignRight().Column(col =>
                {
                    col.Item().Text(t =>
                    {
                        t.Span("Tổng tiền hàng: ").SemiBold();
                        t.Span(FormatMoney(_model.TongTienHang));
                    });

                    if (_model.GiamGia > 0)
                    {
                        col.Item().Text(t =>
                        {
                            t.Span("Giảm giá: ").SemiBold();
                            t.Span(FormatMoney(_model.GiamGia));
                        });
                    }

                    col.Item().Text(t =>
                    {
                        t.Span("Tiền phải trả: ").SemiBold();
                        t.Span(FormatMoney(phaiTra));
                    });

                    col.Item().Text(t =>
                    {
                        t.Span("Đã thanh toán: ").SemiBold();
                        t.Span(FormatMoney(_model.DaThanhToan));
                    });

                    col.Item().Text(t =>
                    {
                        t.Span("Còn nợ: ").SemiBold();
                        t.Span(FormatMoney(conNo));
                    });

                    if (_model.HanThanhToan.HasValue)
                    {
                        col.Item().Text(t =>
                        {
                            t.Span("Hạn thanh toán: ").SemiBold();
                            t.Span(_model.HanThanhToan.Value.ToString("dd/MM/yyyy"));
                        });
                    }
                });

                // ---- GHI CHÚ ----
                if (!string.IsNullOrWhiteSpace(_model.GhiChu))
                {
                    column.Item().PaddingTop(10).Text(t =>
                    {
                        t.Span("Ghi chú: ").SemiBold();
                        t.Span(_model.GhiChu);
                    });
                }

                // ---- CHỮ KÝ ----
                column.Item().PaddingTop(25).Row(row =>
                {
                    row.RelativeItem().AlignCenter().Text(t =>
                    {
                        t.Span("NGƯỜI LẬP PHIẾU").SemiBold();
                    });

                    row.RelativeItem().AlignCenter().Text(t =>
                    {
                        t.Span("ĐẠI DIỆN NHÀ CUNG CẤP").SemiBold();
                    });
                });

                column.Item().Row(row =>
                {
                    row.RelativeItem().AlignCenter().Text("(Ký, ghi rõ họ tên)");
                    row.RelativeItem().AlignCenter().Text("(Ký, ghi rõ họ tên)");
                });
            });
        }

        static string FormatMoney(decimal value)
        {
            return string.Format(ViCulture, "{0:N0} đ", value);
        }
    }
}
