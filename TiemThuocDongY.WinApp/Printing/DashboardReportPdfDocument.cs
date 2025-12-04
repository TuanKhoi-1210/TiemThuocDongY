using System;
using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TiemThuocDongY.Domain.Entities;

namespace TiemThuocDongY.WinApp.Printing
{
    public class DashboardReportPdfDocument : IDocument
    {
        private readonly DashboardSummaryDto _model;
        private static readonly CultureInfo Vi = new("vi-VN");

        public DashboardReportPdfDocument(DashboardSummaryDto model)
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
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("TIỆM THUỐC ĐÔNG Y").SemiBold().FontSize(14);
                        col.Item().Text("BÁO CÁO TỔNG QUAN HOẠT ĐỘNG").Bold().FontSize(18);
                    });

                    row.ConstantItem(120).AlignRight().Column(col =>
                    {
                        col.Item().Text($"Ngày: {DateTime.Now:dd/MM/yyyy}");
                    });
                });
            });
        }

        void ComposeContent(IContainer container)
        {
            container.Column(column =>
            {
                // KPI
                column.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(col =>
                    {
                        col.Item().Text("Doanh thu hôm nay").SemiBold();
                        col.Item().Text(string.Format(Vi, "{0:N0} đ", _model.RevenueToday))
                            .FontSize(16).Bold();
                        col.Item().Text($"{_model.OrdersToday} đơn thuốc");
                    });

                    row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(col =>
                    {
                        col.Item().Text("Đơn thuốc còn nợ").SemiBold();
                        col.Item().Text(_model.PendingOrders.ToString())
                            .FontSize(16).Bold();
                    });

                    row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(col =>
                    {
                        col.Item().Text("Thuốc sắp hết").SemiBold();
                        col.Item().Text(_model.LowStockCount.ToString())
                            .FontSize(16).Bold();
                    });
                });

                // Hoạt động gần đây
                column.Item().PaddingTop(15).Text("Hoạt động gần đây")
                    .Bold().FontSize(13);

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(1.4f); // thời gian
                        cols.RelativeColumn(1);    // loại
                        cols.RelativeColumn(3);    // mô tả
                        cols.RelativeColumn(1.2f); // người thực hiện
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("Thời gian");
                        header.Cell().Element(HeaderCell).Text("Loại");
                        header.Cell().Element(HeaderCell).Text("Mô tả");
                        header.Cell().Element(HeaderCell).Text("Người thực hiện");

                        static IContainer HeaderCell(IContainer c) =>
                            c.Background(Colors.Grey.Lighten3)
                             .Padding(4)
                             .DefaultTextStyle(x => x.SemiBold());
                    });

                    foreach (var act in _model.RecentActivities)
                    {
                        table.Cell().Element(Cell).Text(act.Time.ToString("dd/MM/yyyy HH:mm"));
                        table.Cell().Element(Cell).Text(act.Type);
                        table.Cell().Element(Cell).Text(act.Description);
                        table.Cell().Element(Cell).Text(act.Actor);
                    }

                    static IContainer Cell(IContainer c) =>
                        c.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4);
                });
            });
        }
    }
}
