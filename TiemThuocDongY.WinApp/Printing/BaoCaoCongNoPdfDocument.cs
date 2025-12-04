using System;
using System.Globalization;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TiemThuocDongY.Domain.Entities;

namespace TiemThuocDongY.WinApp.Printing
{
    public class BaoCaoCongNoPdfDocument : IDocument
    {
        private readonly BaoCaoTongHopThangDto _model;
        private static readonly CultureInfo Vi = new("vi-VN");

        public BaoCaoCongNoPdfDocument(BaoCaoTongHopThangDto model)
        {
            _model = model;
        }

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

        void ComposeHeader(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("TIỆM THUỐC ĐÔNG Y").SemiBold().FontSize(14);
                        c.Item().Text("BÁO CÁO CÔNG NỢ").Bold().FontSize(18);
                    });

                    row.ConstantItem(140).AlignRight().Column(c =>
                    {
                        c.Item().Text($"Tháng: {_model.Month:00}/{_model.Year}");
                        c.Item().Text($"Ngày in: {DateTime.Now:dd/MM/yyyy}");
                    });
                });
            });
        }

        void ComposeContent(IContainer container)
        {
            container.Column(column =>
            {
                // Tổng quan công nợ
                var balance = _model.TongPhaiThuKhachHang - _model.TongPhaiTraNhaCungCap;

                column.Item().Row(row =>
                {
                    row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                    {
                        c.Item().Text("Tổng phải thu khách hàng").SemiBold();
                        c.Item().Text(FormatMoney(_model.TongPhaiThuKhachHang))
                            .FontSize(16).Bold();
                        c.Item().Text($"{_model.SoKhachConNo} khách hàng còn nợ");
                    });

                    row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                    {
                        c.Item().Text("Tổng phải trả nhà cung cấp").SemiBold();
                        c.Item().Text(FormatMoney(_model.TongPhaiTraNhaCungCap))
                            .FontSize(16).Bold();
                        c.Item().Text($"{_model.SoNccConNo} nhà cung cấp");
                    });

                    row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                    {
                        c.Item().Text("Chênh lệch (thu - trả)").SemiBold();
                        c.Item().Text(FormatMoney(balance))
                            .FontSize(16).Bold();
                        c.Item().Text(balance >= 0
                            ? "Dương: phải thu > phải trả"
                            : "Âm: phải trả > phải thu");
                    });
                });

                // Công nợ khách hàng
                column.Item().PaddingTop(15).Text("I. Công nợ khách hàng")
                    .Bold().FontSize(13);

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(1.2f); // mã KH
                        cols.RelativeColumn(2);    // tên
                        cols.RelativeColumn(1);    // số đơn
                        cols.RelativeColumn(1.4f); // còn nợ
                        cols.RelativeColumn(1.4f); // hạn TT
                    });

                    table.Header(h =>
                    {
                        h.Cell().Element(HeaderCell).Text("Mã KH");
                        h.Cell().Element(HeaderCell).Text("Khách hàng");
                        h.Cell().Element(HeaderCell).Text("Số đơn còn nợ");
                        h.Cell().Element(HeaderCell).Text("Còn nợ");
                        h.Cell().Element(HeaderCell).Text("Hạn TT gần nhất");

                        static IContainer HeaderCell(IContainer c) =>
                            c.Background(Colors.Grey.Lighten3)
                             .Padding(4)
                             .DefaultTextStyle(x => x.SemiBold());
                    });

                    foreach (var item in _model.CongNoKhachHang.OrderByDescending(x => x.TongNo))
                    {
                        table.Cell().Element(Cell).Text(item.MaKhachHang);
                        table.Cell().Element(Cell).Text(item.HoTen);
                        table.Cell().Element(Cell).AlignCenter().Text(item.SoDonConNo.ToString());
                        table.Cell().Element(Cell).AlignRight().Text(FormatMoney(item.TongNo));
                        table.Cell().Element(Cell).AlignCenter()
                            .Text(item.HanThanhToanGanNhat?.ToString("dd/MM/yyyy") ?? "");
                    }

                    static IContainer Cell(IContainer c) =>
                        c.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4);
                });

                // Công nợ nhà cung cấp
                column.Item().PaddingTop(15).Text("II. Công nợ nhà cung cấp")
                    .Bold().FontSize(13);

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(2);    // nhà cung cấp
                        cols.RelativeColumn(1);    // số phiếu
                        cols.RelativeColumn(1.4f); // còn nợ
                        cols.RelativeColumn(1.4f); // hạn TT
                    });

                    table.Header(h =>
                    {
                        h.Cell().Element(HeaderCell).Text("Nhà cung cấp");
                        h.Cell().Element(HeaderCell).Text("Số phiếu còn nợ");
                        h.Cell().Element(HeaderCell).Text("Còn nợ");
                        h.Cell().Element(HeaderCell).Text("Hạn TT gần nhất");

                        static IContainer HeaderCell(IContainer c) =>
                            c.Background(Colors.Grey.Lighten3)
                             .Padding(4)
                             .DefaultTextStyle(x => x.SemiBold());
                    });

                    foreach (var item in _model.CongNoNhaCungCap.OrderByDescending(x => x.TongNo))
                    {
                        table.Cell().Element(Cell).Text(item.TenNCC);
                        table.Cell().Element(Cell).AlignCenter().Text(item.SoPhieuConNo.ToString());
                        table.Cell().Element(Cell).AlignRight().Text(FormatMoney(item.TongNo));
                        table.Cell().Element(Cell).AlignCenter()
                            .Text(item.HanThanhToanGanNhat?.ToString("dd/MM/yyyy") ?? "");
                    }

                    static IContainer Cell(IContainer c) =>
                        c.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4);
                });
            });
        }

        static string FormatMoney(decimal v) =>
            string.Format(Vi, "{0:N0} đ", v);
    }
}
