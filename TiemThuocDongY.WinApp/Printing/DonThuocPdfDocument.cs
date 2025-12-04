using System;
using System.Globalization;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TiemThuocDongY.Domain.Entities; // nơi chứa DonThuocDetailDto

namespace TiemThuocDongY.WinApp.Printing
{
    public class DonThuocPdfDocument : IDocument
    {
        private readonly DonThuocDetailDto _model;
        private static readonly CultureInfo ViCulture = new("vi-VN");

        public DonThuocPdfDocument(DonThuocDetailDto model)
        {
            _model = model;
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
                        col.Item().Text($"Số đơn: {_model.SoDon}")
                            .SemiBold();
                        col.Item().Text($"Ngày lập: {_model.NgayLap:dd/MM/yyyy}");
                    });
                });

                column.Item().PaddingTop(15).AlignCenter().Text(text =>
                {
                    text.Span("ĐƠN THUỐC ĐÔNG Y")
                        .Bold().FontSize(18).Underline();
                });

                column.Item().PaddingTop(10).Text(txt =>
                {
                    txt.Span("Bệnh nhân: ").SemiBold();
                    txt.Span($"{_model.TenKhachHang} ({_model.MaKhachHang})");
                });

                column.Item().Text(txt =>
                {
                    txt.Span("Năm sinh: ").SemiBold();
                    txt.Span(_model.NamSinh?.ToString() ?? "");
                    txt.Span("    Địa chỉ: ").SemiBold();
                    txt.Span(_model.DiaChi ?? "");
                });

                column.Item().Text(txt =>
                {
                    txt.Span("Bác sĩ kê đơn: ").SemiBold();
                    txt.Span(_model.BacSiKeDon ?? "");
                });

                column.Item().PaddingTop(5).Text(txt =>
                {
                    txt.Span("Chẩn đoán: ").SemiBold();
                    txt.Span(_model.ChanDoan ?? "");
                });
            });
        }

        // ================= CONTENT =================
        void ComposeContent(IContainer container)
        {
            var phaiTra = _model.TienKhachPhaiTra;
            if (phaiTra <= 0)
                phaiTra = _model.TongTienHang - _model.GiamGia;

            var conNo = _model.ConNo;

            container.PaddingTop(15).Column(column =>
            {
                // ---- BẢNG THUỐC ----
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(25);   // STT
                        columns.RelativeColumn(3);    // Tên thuốc
                        columns.RelativeColumn(1);    // Liều
                        columns.RelativeColumn(1);    // Số thang
                        columns.RelativeColumn(1.5f); // Đơn giá
                        columns.RelativeColumn(1.5f); // Thành tiền
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("STT");
                        header.Cell().Element(HeaderCell).Text("Tên thuốc");
                        header.Cell().Element(HeaderCell).Text("Liều (gram)");
                        header.Cell().Element(HeaderCell).Text("Số thang");
                        header.Cell().Element(HeaderCell).Text("Đơn giá");
                        header.Cell().Element(HeaderCell).Text("Thành tiền");

                        static IContainer HeaderCell(IContainer container) =>
                            container.DefaultTextStyle(x => x.SemiBold())
                                     .Padding(4)
                                     .Background(Colors.Grey.Lighten3)
                                     .BorderBottom(1)
                                     .BorderColor(Colors.Grey.Darken2);
                    });

                    // Rows
                    var lines = _model.ChiTiet ?? Enumerable.Empty<DonThuocDetailLineDto>();

                    int index = 1;
                    foreach (var line in lines)
                    {
                        table.Cell().Element(Cell).AlignCenter().Text(index.ToString());
                        table.Cell().Element(Cell).Text(line.TenThuoc);
                        table.Cell().Element(Cell).AlignRight()
                            .Text(line.LieuLuongGram.ToString("0.##", ViCulture));
                        table.Cell().Element(Cell).AlignRight()
                            .Text(line.SoThang.ToString());
                        table.Cell().Element(Cell).AlignRight()
                            .Text(FormatMoney(line.DonGiaBan));
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
                        t.Span("Tổng tiền thuốc: ").SemiBold();
                        t.Span(FormatMoney(_model.TongTienHang));
                    });
                    col.Item().Text(t =>
                    {
                        t.Span("Giảm giá: ").SemiBold();
                        t.Span(FormatMoney(_model.GiamGia));
                    });
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
                        t.Span("BỆNH NHÂN").SemiBold();
                    });

                    row.RelativeItem().AlignCenter().Text(t =>
                    {
                        t.Span("THẦY THUỐC").SemiBold();
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
