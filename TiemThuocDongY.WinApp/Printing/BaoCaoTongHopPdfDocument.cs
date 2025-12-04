using System;
using System.Globalization;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TiemThuocDongY.Domain.Entities;

namespace TiemThuocDongY.WinApp.Printing
{
    public class BaoCaoTongHopPdfDocument : IDocument
    {
        private readonly BaoCaoTongHopThangDto _model;
        private static readonly CultureInfo Vi = new("vi-VN");

        public BaoCaoTongHopPdfDocument(BaoCaoTongHopThangDto model)
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
                        c.Item().Text("BÁO CÁO TỔNG HỢP").Bold().FontSize(18);
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
                // I. Tổng quan doanh thu & kho
                column.Item().Text("I. Tổng quan doanh thu và nhập kho")
                    .Bold().FontSize(13);

                column.Item().Row(row =>
                {
                    row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                    {
                        c.Item().Text("Doanh thu hôm nay").SemiBold();
                        c.Item().Text(FormatMoney(_model.DoanhThuHomNay))
                            .FontSize(14).Bold();
                        c.Item().Text($"{_model.SoDonHomNay} đơn thuốc");
                    });

                    row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                    {
                        c.Item().Text("Doanh thu tháng").SemiBold();
                        c.Item().Text(FormatMoney(_model.DoanhThuThang))
                            .FontSize(14).Bold();
                        c.Item().Text($"{_model.SoDonThang} đơn thuốc");
                    });

                    row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                    {
                        c.Item().Text("Giá trị nhập kho tháng").SemiBold();
                        c.Item().Text(FormatMoney(_model.GiaTriNhapThang))
                            .FontSize(14).Bold();
                        c.Item().Text($"{_model.SoPhieuNhapThang} phiếu nhập");
                    });
                });

                // II. Doanh thu 7 ngày + Top thuốc
                column.Item().PaddingTop(15).Text("II. Doanh thu 7 ngày gần nhất")
                    .Bold().FontSize(13);

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(1.2f);
                        cols.RelativeColumn(1);
                        cols.RelativeColumn(1.2f);
                    });

                    table.Header(h =>
                    {
                        h.Cell().Element(HeaderCell).Text("Ngày");
                        h.Cell().Element(HeaderCell).Text("Số đơn");
                        h.Cell().Element(HeaderCell).Text("Doanh thu");

                        static IContainer HeaderCell(IContainer c) =>
                            c.Background(Colors.Grey.Lighten3)
                             .Padding(4)
                             .DefaultTextStyle(x => x.SemiBold());
                    });

                    foreach (var d in _model.DoanhThu7Ngay.OrderBy(x => x.Ngay))
                    {
                        table.Cell().Element(Cell).Text(d.Ngay.ToString("dd/MM/yyyy"));
                        table.Cell().Element(Cell).AlignCenter().Text(d.SoDon.ToString());
                        table.Cell().Element(Cell).AlignRight().Text(FormatMoney(d.DoanhThu));
                    }

                    static IContainer Cell(IContainer c) =>
                        c.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4);
                });

                column.Item().PaddingTop(10).Text("III. Thuốc bán chạy trong tháng")
                    .Bold().FontSize(13);

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(20);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(1);
                        cols.RelativeColumn(1.2f);
                        cols.RelativeColumn(1.2f);
                    });

                    table.Header(h =>
                    {
                        h.Cell().Element(HeaderCell2).Text("#");
                        h.Cell().Element(HeaderCell2).Text("Thuốc");
                        h.Cell().Element(HeaderCell2).Text("Đơn vị");
                        h.Cell().Element(HeaderCell2).Text("Số lượng bán");
                        h.Cell().Element(HeaderCell2).Text("Doanh thu");

                        static IContainer HeaderCell2(IContainer c) =>
                            c.Background(Colors.Grey.Lighten3)
                             .Padding(4)
                             .DefaultTextStyle(x => x.SemiBold());
                    });

                    int i = 1;
                    foreach (var item in _model.TopThuocThang)
                    {
                        table.Cell().Element(Cell2).AlignCenter().Text(i.ToString());
                        table.Cell().Element(Cell2).Text(item.TenThuoc);
                        table.Cell().Element(Cell2).AlignCenter().Text(item.DonViTinh);
                        table.Cell().Element(Cell2).AlignRight()
                            .Text(item.SoLuongBan.ToString("0.##", Vi));
                        table.Cell().Element(Cell2).AlignRight()
                            .Text(FormatMoney(item.DoanhThu));
                        i++;
                    }

                    static IContainer Cell2(IContainer c) =>
                        c.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4);
                });

                // IV. Công nợ tổng quan
                column.Item().PaddingTop(15).Text("IV. Tổng hợp công nợ")
                    .Bold().FontSize(13);

                var balance = _model.TongPhaiThuKhachHang - _model.TongPhaiTraNhaCungCap;

                column.Item().Row(row =>
                {
                    row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                    {
                        c.Item().Text("Phải thu khách hàng").SemiBold();
                        c.Item().Text(FormatMoney(_model.TongPhaiThuKhachHang))
                            .FontSize(14).Bold();
                        c.Item().Text($"{_model.SoKhachConNo} khách hàng còn nợ");
                    });

                    row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                    {
                        c.Item().Text("Phải trả nhà cung cấp").SemiBold();
                        c.Item().Text(FormatMoney(_model.TongPhaiTraNhaCungCap))
                            .FontSize(14).Bold();
                        c.Item().Text($"{_model.SoNccConNo} nhà cung cấp");
                    });

                    row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                    {
                        c.Item().Text("Chênh lệch (thu - trả)").SemiBold();
                        c.Item().Text(FormatMoney(balance))
                            .FontSize(14).Bold();
                        c.Item().Text(balance >= 0
                            ? "Dương: phải thu > phải trả"
                            : "Âm: phải trả > phải thu");
                    });
                });
            });
        }

        static string FormatMoney(decimal v) =>
            string.Format(Vi, "{0:N0} đ", v);
    }
}
