using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using QuestPDF.Fluent;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms;
using TiemThuocDongY.Data;
using TiemThuocDongY.Data.Repositories;
using TiemThuocDongY.Domain.Entities;
using TiemThuocDongY.Services;
using TiemThuocDongY.Services.Auth;
using TiemThuocDongY.Services.Email;
using TiemThuocDongY.Services.ThuocSvc;
using TiemThuocDongY.WinApp.Printing;
namespace TiemThuocDongY.WinApp   
{
    public partial class MainForm : Form
    {
        private readonly AuthService _authService;
        private readonly ThuocService _thuocService;
        private readonly UserRepository _userRepository;
        private readonly KhachHangRepository _khachRepository;
        private readonly DonThuocRepository _donThuocRepository;
        private readonly PhieuNhapRepository _phieuNhapRepository;
        private int _currentUserId;
        private readonly NhaCungCapRepository _nccRepository;
        private readonly DashboardService _dashboardService;
        private readonly BaoCaoService _baoCaoService;
        public MainForm()
        {
            InitializeComponent();


            _userRepository = new UserRepository();

            var emailSettings = new EmailSettings
            {
                SmtpHost = "smtp.gmail.com",
                SmtpPort = 587,
                EnableSsl = true,
                UserName = "khoikhodo7@gmail.com",        // Gmail dùng để gửi
                Password = "zxrd urye nzye bapy",      // App Password (không phải password login)
                FromAddress = "khoikhodo7@gmail.com",
                FromDisplayName = "Tiệm Thuốc Đông Y"
            };

            var emailService = new EmailService(emailSettings);

            _authService = new AuthService(_userRepository, emailService);
            _thuocService = new ThuocService(new ThuocRepository());
            _khachRepository = new KhachHangRepository();
            _donThuocRepository = new DonThuocRepository();
            _phieuNhapRepository = new PhieuNhapRepository();
            _nccRepository = new NhaCungCapRepository();
            _dashboardService = new DashboardService(new DashboardRepository());
            _baoCaoService = new BaoCaoService(new BaoCaoRepository());
            InitializeWebViewAsync();
        }

        private async void InitializeWebViewAsync()
        {
            await webView21.EnsureCoreWebView2Async(null);

            var root = Path.Combine(Application.StartupPath, "wwwroot");
            var indexPath = Path.Combine(root, "index.html");
            webView21.Source = new Uri(indexPath);

            webView21.CoreWebView2.WebMessageReceived += WebView2_WebMessageReceived;
        }

        private async void WebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var json = e.WebMessageAsJson;

            WebMessage? msg;
            try
            {
                msg = JsonSerializer.Deserialize<WebMessage>(json);
            }
            catch
            {
                return;
            }

            if (msg == null) return;

            switch (msg.Action)
            {
                case "login":
                    await HandleLoginAsync(msg.Data);
                    break;
                case "getThuocList":
                    await HandleGetThuocListAsync();
                    break;
                case "getKhachList":                     // ✨ thêm
                    await HandleGetKhachListAsync();
                    break;
                case "getUserList":
                    await HandleGetUserListAsync();
                    break;
                case "updateUser":
                    await HandleUpdateUserAsync(msg.Data);
                    break;
                case "setUserActive":
                    await HandleSetUserActiveAsync(msg.Data);
                    break;
                case "deleteUser":
                    await HandleDeleteUserAsync(msg.Data);
                    break;
                case "createThuoc":
                    await HandleCreateThuocAsync(msg.Data);
                    break;
                case "updateThuoc":
                    await HandleUpdateThuocAsync(msg.Data);
                    break;
                case "deleteThuoc":
                    await HandleDeleteThuocAsync(msg.Data);
                    break;
                case "createUser":
                    await HandleCreateUserAsync(msg.Data);
                    break;
                case "createKhach":
                    await HandleCreateKhachAsync(msg.Data);
                    break;

                case "updateKhach":
                    await HandleUpdateKhachAsync(msg.Data);
                    break;
                case "deleteKhach":
                    await HandleDeleteKhachAsync(msg.Data);
                    break;
                case "createDonThuoc":
                    {
                        // Deserialize DTO giống code cũ, nhưng cho phép case-insensitive cho chắc
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        var dto = JsonSerializer.Deserialize<DonThuocCreateDto>(msg.Data.GetRawText(), options);
                        if (dto == null)
                        {
                            MessageBox.Show("Không đọc được dữ liệu tạo đơn thuốc.", "Lỗi");
                            break;
                        }

                        var service = new DonThuocService();
                        var id = service.Create(dto.Header, dto.Details);

                        // ❗ Chỉ truyền tên hàm, KHÔNG thêm 'App.'
                        await CallJsAsync("onDonThuocCreated", new { DonThuocId = id });
                        break;
                    }

                case "updateDonThuoc":
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        var dto = JsonSerializer.Deserialize<DonThuocCreateDto>(msg.Data.GetRawText(), options);
                        if (dto == null)
                        {
                            MessageBox.Show("Không đọc được dữ liệu cập nhật đơn thuốc.", "Lỗi");
                            break;
                        }

                        var service = new DonThuocService();
                        service.Update(dto.Header, dto.Details);

                        await CallJsAsync("onDonThuocUpdated", new { Success = true });
                        break;
                    }

                case "loadDonThuocForEdit":
                    {
                        int id = msg.Data.GetProperty("DonThuocId").GetInt32();
                        var service = new DonThuocService();

                        var detail = service.GetDetail(id);

                        // ❗ Chỉ 'onDonThuocLoadForEdit'
                        await CallJsAsync("onDonThuocLoadForEdit", detail);
                        break;
                    }


                case "getDonThuocList":
                    await HandleGetDonThuocListAsync();
                    break;
                case "getDonThuocDetail":
                    await HandleGetDonThuocDetailAsync(msg.Data);
                    break;
                case "passwordReset_request_login":
                case "passwordReset_request_account":
                    await HandlePasswordResetRequestAsync(msg.Data,
                        msg.Action == "passwordReset_request_login"
                            ? "onForgotPasswordCodeSentLogin"
                            : "onForgotPasswordCodeSentAccount");
                    break;
                case "completeDonThuoc":
                    {
                        // Không cần DonThuocIdDto, lấy thẳng từ JsonElement
                        int id = msg.Data.GetProperty("DonThuocId").GetInt32();
                        await HandleCompleteDonThuocAsync(id);
                        break;
                    }
                case "deleteDonThuoc":
                    {
                        int id = msg.Data.GetProperty("DonThuocId").GetInt32();
                        await HandleDeleteDonThuocAsync(id);
                        break;
                    }
                case "printDonThuoc":
                    {
                        int id = msg.Data.GetProperty("DonThuocId").GetInt32();
                        await HandlePrintDonThuocAsync(id);
                        break;
                    }
                case "getDonThuocForPayment":
                    {
                        int id = msg.Data.GetProperty("DonThuocId").GetInt32();
                        await HandleGetDonThuocForPaymentAsync(id);
                        break;
                    }

                case "thuTienDonThuoc":
                    {
                        var data = msg.Data;

                        int id = data.GetProperty("DonThuocId").GetInt32();

                        decimal soTien = 0;
                        if (data.TryGetProperty("SoTien", out var soTienEl) &&
                            soTienEl.ValueKind == System.Text.Json.JsonValueKind.Number)
                        {
                            try { soTien = soTienEl.GetDecimal(); }
                            catch { soTien = (decimal)soTienEl.GetDouble(); }
                        }

                        byte hinhThuc = 0;
                        if (data.TryGetProperty("HinhThucThanhToan", out var htEl) &&
                            htEl.ValueKind == System.Text.Json.JsonValueKind.Number)
                        {
                            hinhThuc = (byte)htEl.GetInt32();
                        }

                        string ghiChu = "";
                        if (data.TryGetProperty("GhiChu", out var gcEl) &&
                            gcEl.ValueKind == System.Text.Json.JsonValueKind.String)
                        {
                            ghiChu = gcEl.GetString() ?? "";
                        }

                        await HandleThuTienDonThuocAsync(id, soTien, hinhThuc, ghiChu);
                        break;
                    }
                case "getPhieuNhapList":
                    await HandleGetPhieuNhapListAsync();
                    break;
                case "getPhieuNhapDetail":
                    await HandleGetPhieuNhapDetailAsync(msg.Data);
                    break;
                case "createPhieuNhap":
                    await HandleCreatePhieuNhapAsync(msg.Data);
                    break;
                case "payPhieuNhap":
                    await HandlePayPhieuNhapAsync(msg.Data);
                    break;
                case "printPhieuNhap":
                    {
                        int id = msg.Data.GetProperty("phieuNhapId").GetInt32();
                        await HandlePrintPhieuNhapAsync(id);
                        break;
                    }
                case "getNccList":
                    await HandleGetNccListAsync();
                    break;
                case "createNcc":
                    await HandleCreateNccAsync(msg.Data);
                    break;
                case "updateNcc":
                    await HandleUpdateNccAsync(msg.Data);
                    break;
                case "deleteNcc":
                    await HandleDeleteNccAsync(msg.Data);
                    break;
                case "getDashboardSummary":
                    await HandleGetDashboardSummaryAsync();
                    break;

                case "exportDashboardReport":
                    await HandleExportDashboardReportAsync();
                    break;
                case "getBaoCaoThang":
                    {
                        int year = msg.Data.GetProperty("year").GetInt32();
                        int month = msg.Data.GetProperty("month").GetInt32();
                        await HandleGetBaoCaoThangAsync(year, month);
                        break;
                    }
                case "exportBaoCaoKho":
                    {
                        int year = msg.Data.GetProperty("year").GetInt32();
                        int month = msg.Data.GetProperty("month").GetInt32();
                        await HandleExportBaoCaoKhoAsync(year, month);
                        break;
                    }
                case "exportBaoCaoCongNo":
                    {
                        int year = msg.Data.GetProperty("year").GetInt32();
                        int month = msg.Data.GetProperty("month").GetInt32();
                        await HandleExportBaoCaoCongNoAsync(year, month);
                        break;
                    }
                case "exportBaoCaoTongHop":
                    {
                        int year = msg.Data.GetProperty("year").GetInt32();
                        int month = msg.Data.GetProperty("month").GetInt32();
                        await HandleExportBaoCaoTongHopAsync(year, month);
                        break;
                    }

                case "passwordReset_confirm_login":
                case "passwordReset_confirm_account":
                    await HandlePasswordResetConfirmAsync(msg.Data,
                        msg.Action == "passwordReset_confirm_login"
                            ? "onForgotPasswordDoneLogin"
                            : "onForgotPasswordDoneAccount");
                    break;

            }
        }
        private Task CallJsAsync(string functionName, object payload)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(payload, options);
            var script = $"window.App && App.{functionName}({json});";
            return webView21.CoreWebView2.ExecuteScriptAsync(script);
        }
        private async Task HandleGetDonThuocForPaymentAsync(int donThuocId)
        {
            try
            {
                var detail = _donThuocRepository.GetDetail(donThuocId);
                await CallJsAsync("onDonThuocForPayment", detail);
            }
            catch (Exception ex)
            {
                await CallJsAsync("onDonThuocForPayment", null);
                MessageBox.Show(ex.ToString(), "Lỗi load đơn thuốc để thu tiền");
            }
        }
        private async Task HandleGetNccListAsync()
        {
            try
            {
                var list = _nccRepository.GetAll();
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var json = JsonSerializer.Serialize(list, options);
                var script = $"window.App && App.onNccList({json});";
                await webView21.CoreWebView2.ExecuteScriptAsync(script);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Lỗi load nhà cung cấp");
            }
        }
        private async Task HandlePrintPhieuNhapAsync(int phieuNhapId)
        {
            try
            {
                var detail = _phieuNhapRepository.GetDetail(phieuNhapId);
                if (detail == null)
                {
                    await CallJsAsync("onPrintPhieuNhapResult", new
                    {
                        success = false,
                        message = "Không tìm thấy phiếu nhập."
                    });
                    return;
                }

                var doc = new PhieuNhapPdfDocument(detail);

                // Tạo file PDF tạm trong Documents
                var fileName = $"PhieuNhap_{detail.SoPhieu}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var path = Path.Combine(folder, fileName);

                var pdfBytes = doc.GeneratePdf();
                File.WriteAllBytes(path, pdfBytes);

                // Mở file bằng PDF viewer mặc định của Windows
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });

                await CallJsAsync("onPrintPhieuNhapResult", new
                {
                    success = true,
                    filePath = path
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Lỗi in phiếu nhập");
                await CallJsAsync("onPrintPhieuNhapResult", new
                {
                    success = false,
                    message = "In phiếu nhập thất bại."
                });
            }
        }
        private async Task HandleGetBaoCaoThangAsync(int year, int month)
        {
            var dto = _baoCaoService.GetTongHopThang(year, month);

            await CallJsAsync("onBaoCaoThangResult", new
            {
                year = dto.Year,
                month = dto.Month,
                doanhThuHomNay = dto.DoanhThuHomNay,
                soDonHomNay = dto.SoDonHomNay,
                doanhThuThang = dto.DoanhThuThang,
                doanhThuThangTruoc = dto.DoanhThuThangTruoc,
                soDonThang = dto.SoDonThang,
                giaTriNhapThang = dto.GiaTriNhapThang,
                soPhieuNhapThang = dto.SoPhieuNhapThang,

                doanhThu7Ngay = dto.DoanhThu7Ngay.Select(x => new
                {
                    ngay = x.Ngay,
                    soDon = x.SoDon,
                    doanhThu = x.DoanhThu
                }).ToList(),

                topThuoc = dto.TopThuocThang.Select(x => new
                {
                    tenThuoc = x.TenThuoc,
                    donViTinh = x.DonViTinh,
                    soLuongBan = x.SoLuongBan,
                    doanhThu = x.DoanhThu
                }).ToList(),

                tongPhaiThu = dto.TongPhaiThuKhachHang,
                soKhachConNo = dto.SoKhachConNo,
                tongPhaiTra = dto.TongPhaiTraNhaCungCap,
                soNccConNo = dto.SoNccConNo,

                congNoKh = dto.CongNoKhachHang.Select(x => new
                {
                    maKhachHang = x.MaKhachHang,
                    hoTen = x.HoTen,
                    tongNo = x.TongNo,
                    soDonConNo = x.SoDonConNo,
                    hanThanhToanGanNhat = x.HanThanhToanGanNhat
                }).ToList(),

                congNoNcc = dto.CongNoNhaCungCap.Select(x => new
                {
                    tenNcc = x.TenNCC,
                    maNcc = x.MaNCC,
                    tongNo = x.TongNo,
                    soPhieuConNo = x.SoPhieuConNo,
                    hanThanhToanGanNhat = x.HanThanhToanGanNhat
                }).ToList()
            });
        }
        private async Task HandleExportBaoCaoKhoAsync(int year, int month)
        {
            try
            {
                var dto = _baoCaoService.GetTongHopThang(year, month);
                var doc = new BaoCaoKhoPdfDocument(dto);

                var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var fileName = $"BaoCaoKho_{year}_{month:00}.pdf";
                var path = Path.Combine(folder, fileName);

                var bytes = doc.GeneratePdf();
                File.WriteAllBytes(path, bytes);

                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });

                await CallJsAsync("onExportBaoCaoResult", new
                {
                    success = true,
                    type = "stock",
                    filePath = path
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Lỗi xuất báo cáo kho");

                await CallJsAsync("onExportBaoCaoResult", new
                {
                    success = false,
                    type = "stock",
                    message = "Xuất báo cáo kho thất bại."
                });
            }
        }

        private async Task HandleExportBaoCaoCongNoAsync(int year, int month)
        {
            try
            {
                var dto = _baoCaoService.GetTongHopThang(year, month);
                var doc = new BaoCaoCongNoPdfDocument(dto);

                var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var fileName = $"BaoCaoCongNo_{year}_{month:00}.pdf";
                var path = Path.Combine(folder, fileName);

                var bytes = doc.GeneratePdf();
                File.WriteAllBytes(path, bytes);

                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });

                await CallJsAsync("onExportBaoCaoResult", new
                {
                    success = true,
                    type = "debt",
                    filePath = path
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Lỗi xuất báo cáo công nợ");

                await CallJsAsync("onExportBaoCaoResult", new
                {
                    success = false,
                    type = "debt",
                    message = "Xuất báo cáo công nợ thất bại."
                });
            }
        }

        private async Task HandleExportBaoCaoTongHopAsync(int year, int month)
        {
            try
            {
                var dto = _baoCaoService.GetTongHopThang(year, month);
                var doc = new BaoCaoTongHopPdfDocument(dto);

                var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var fileName = $"BaoCaoTongHop_{year}_{month:00}.pdf";
                var path = Path.Combine(folder, fileName);

                var bytes = doc.GeneratePdf();
                File.WriteAllBytes(path, bytes);

                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });

                await CallJsAsync("onExportBaoCaoResult", new
                {
                    success = true,
                    type = "summary",
                    filePath = path
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Lỗi xuất báo cáo tổng hợp");

                await CallJsAsync("onExportBaoCaoResult", new
                {
                    success = false,
                    type = "summary",
                    message = "Xuất báo cáo tổng hợp thất bại."
                });
            }
        }

        private async Task HandleGetDashboardSummaryAsync()
        {
            var summary = _dashboardService.GetTodaySummary();

            await CallJsAsync("onDashboardSummary", new
            {
                revenueToday = summary.RevenueToday,
                ordersToday = summary.OrdersToday,
                pendingOrders = summary.PendingOrders,
                lowStockCount = summary.LowStockCount,
                recentActivities = summary.RecentActivities.Select(a => new
                {
                    time = a.Time,
                    type = a.Type,
                    description = a.Description,
                    actor = a.Actor
                }).ToList()
            });
        }

        private async Task HandleExportDashboardReportAsync()
        {
            try
            {
                var summary = _dashboardService.GetTodaySummary();
                var doc = new DashboardReportPdfDocument(summary);

                var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var fileName = $"BaoCaoTongQuan_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                var path = Path.Combine(folder, fileName);

                var bytes = doc.GeneratePdf();
                File.WriteAllBytes(path, bytes);

                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });

                await CallJsAsync("onExportDashboardReportResult", new
                {
                    success = true,
                    filePath = path
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Lỗi xuất báo cáo dashboard");

                await CallJsAsync("onExportDashboardReportResult", new
                {
                    success = false,
                    message = "Xuất báo cáo tổng quan thất bại."
                });
            }
        }



        private async Task HandleCreateNccAsync(JsonElement data)
        {
            try
            {
                var ncc = ParseNccFromJson(data);
                int newId = _nccRepository.Insert(ncc, _currentUserId);

                // reload list cho cả màn NCC + phiếu nhập
                await HandleGetNccListAsync();

                await CallJsAsync("onNccSaved", new
                {
                    success = true,
                    nhaCungCapId = newId
                });
            }
            catch (Exception ex)
            {
                await CallJsAsync("onNccSaved", new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        private async Task HandleUpdateNccAsync(JsonElement data)
        {
            try
            {
                var ncc = ParseNccFromJson(data);
                _nccRepository.Update(ncc);

                await HandleGetNccListAsync();
                await CallJsAsync("onNccSaved", new
                {
                    success = true,
                    nhaCungCapId = ncc.NhaCungCapId
                });
            }
            catch (Exception ex)
            {
                await CallJsAsync("onNccSaved", new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        private async Task HandleDeleteNccAsync(JsonElement data)
        {
            try
            {
                int id = data.GetProperty("nhaCungCapId").GetInt32();
                _nccRepository.Delete(id);
                await HandleGetNccListAsync();
                await CallJsAsync("onNccDeleted", new { success = true });
            }
            catch (Exception ex)
            {
                await CallJsAsync("onNccDeleted", new { success = false, message = ex.Message });
            }
        }

        private DM_NhaCungCap ParseNccFromJson(JsonElement data)
        {
            var id = data.TryGetProperty("nhaCungCapId", out var idEl) ? idEl.GetInt32() : 0;
            var ma = data.GetProperty("maNcc").GetString() ?? "";
            var ten = data.GetProperty("tenNcc").GetString() ?? "";

            var nguoiLienHe = data.TryGetProperty("nguoiLienHe", out var nlEl)
                ? nlEl.GetString() ?? ""
                : "";
            var dienThoai = data.TryGetProperty("dienThoai", out var dtEl)
                ? dtEl.GetString() ?? ""
                : "";
            var email = data.TryGetProperty("email", out var emEl)
                ? emEl.GetString() ?? ""
                : "";
            var diaChi = data.TryGetProperty("diaChi", out var dcEl)
                ? dcEl.GetString() ?? ""
                : "";
            var ghiChu = data.TryGetProperty("ghiChu", out var gcEl)
                ? gcEl.GetString() ?? ""
                : "";

            return new DM_NhaCungCap
            {
                NhaCungCapId = id,
                MaNcc = ma,
                TenNcc = ten,
                NguoiLienHe = nguoiLienHe,
                DienThoai = dienThoai,
                Email = email,
                DiaChi = diaChi,
                GhiChu = ghiChu
            };
        }

        private async Task HandlePasswordResetRequestAsync(JsonElement data, string jsCallback)
        {
            var userName = data.GetProperty("userName").GetString() ?? string.Empty;
            var result = _authService.RequestPasswordReset(userName);
            await CallJsAsync(jsCallback, result);
        }
        private async Task HandleDeleteDonThuocAsync(int donThuocId)
        {
            bool success = true;
            string message;

            try
            {
                var service = new DonThuocService();
                service.Delete(donThuocId);
                message = "Đã xóa đơn thuốc.";
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
            }

            await CallJsAsync("onDeleteDonThuocResult", new
            {
                success,
                message
            });
        }
        private async Task HandleGetPhieuNhapListAsync()
        {
            try
            {
                var list = _phieuNhapRepository.GetAllForListing();
                await CallJsAsync("onPhieuNhapList", list);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Lỗi load phiếu nhập");
            }
        }

        private async Task HandleGetPhieuNhapDetailAsync(JsonElement data)
        {
            try
            {
                int id = data.GetProperty("phieuNhapId").GetInt32();
                var detail = _phieuNhapRepository.GetDetail(id);
                await CallJsAsync("onPhieuNhapDetail", detail);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Lỗi xem phiếu nhập");
            }
        }

        private async Task HandleCreatePhieuNhapAsync(JsonElement data)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var dto = JsonSerializer.Deserialize<PhieuNhapCreateDto>(data.GetRawText(), options);
                if (dto == null)
                {
                    MessageBox.Show("Không đọc được dữ liệu phiếu nhập.", "Lỗi");
                    return;
                }

                // ràng buộc đơn giản
                if (dto.Details == null || dto.Details.Count == 0)
                    throw new Exception("Phiếu nhập phải có ít nhất 1 dòng thuốc.");

                foreach (var d in dto.Details)
                {
                    if (d.SoLuong <= 0)
                        throw new Exception("Số lượng nhập phải > 0.");
                    if (d.DonGiaNhap < 0)
                        throw new Exception("Đơn giá nhập không được âm.");
                }

                var newId = _phieuNhapRepository.Insert(dto.Header, dto.Details, _currentUserId);


                await CallJsAsync("onPhieuNhapSaved", new { success = true, phieuNhapId = newId });
                await HandleGetPhieuNhapListAsync(); // reload list
            }
            catch (Exception ex)
            {
                await CallJsAsync("onPhieuNhapSaved", new { success = false, message = ex.Message });
            }
        }

        private async Task HandlePayPhieuNhapAsync(JsonElement data)
        {
            try
            {
                int id = data.GetProperty("phieuNhapId").GetInt32();
                decimal soTien = (decimal)data.GetProperty("soTien").GetDouble();
                byte hinhThuc = (byte)data.GetProperty("hinhThuc").GetInt32();
                string ghiChu = data.TryGetProperty("ghiChu", out var gcEl) ? (gcEl.GetString() ?? "") : "";

                _phieuNhapRepository.AddPayment(id, soTien, hinhThuc, ghiChu, _currentUserId);


                await CallJsAsync("onPhieuNhapPaid", new { success = true });
                await HandleGetPhieuNhapListAsync();
            }
            catch (Exception ex)
            {
                await CallJsAsync("onPhieuNhapPaid", new { success = false, message = ex.Message });
            }
        }

        private async Task HandleThuTienDonThuocAsync(int donThuocId, decimal soTien, byte hinhThucThanhToan, string ghiChu)
        {
            bool success = true;
            string message;

            try
            {
                if (soTien <= 0)
                    throw new Exception("Số tiền thu phải lớn hơn 0.");

                _donThuocRepository.ThuTienDonThuoc(donThuocId, soTien, hinhThucThanhToan, ghiChu);
                message = "Đã ghi nhận phiếu thu.";
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
            }

            await CallJsAsync("onThuTienDonThuocResult", new
            {
                success,
                message
            });
        }


        private async Task HandleCompleteDonThuocAsync(int donThuocId)
        {
            bool success = true;
            string message;

            try
            {
                var service = new DonThuocService();
                service.MarkAsCompleted(donThuocId);

                message = "Đã chuyển đơn sang trạng thái 'Đã kê đơn'.";
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
            }

            await CallJsAsync("onCompleteDonThuocResult", new
            {
                success,
                message
            });
        }



        private DM_KhachHang ParseKhachFromJson(JsonElement data)
        {
            // Id: chỉ có khi sửa, thêm mới thì = 0
            var id = 0;
            if (data.TryGetProperty("khachHangId", out var idEl) &&
                idEl.ValueKind == JsonValueKind.Number)
            {
                id = idEl.GetInt32();
            }

            // MaKhachHang: giờ để optional, repo sẽ tự sinh khi Insert
            string ma = "";
            if (data.TryGetProperty("maKhachHang", out var maEl) &&
                maEl.ValueKind == JsonValueKind.String)
            {
                ma = maEl.GetString() ?? "";
            }

            // HoTen (bắt buộc ở UI, nhưng vẫn nên TryGetProperty cho an toàn)
            string hoTen = "";
            if (data.TryGetProperty("hoTen", out var tenEl) &&
                tenEl.ValueKind == JsonValueKind.String)
            {
                hoTen = tenEl.GetString() ?? "";
            }

            // NamSinh: nullable
            int? namSinh = null;
            if (data.TryGetProperty("namSinh", out var nsEl) &&
                nsEl.ValueKind == JsonValueKind.Number)
            {
                namSinh = nsEl.GetInt32();
            }

            // GioiTinh: nullable (0/1)
            byte? gioiTinh = null;
            if (data.TryGetProperty("gioiTinh", out var gtEl) &&
                gtEl.ValueKind == JsonValueKind.Number)
            {
                gioiTinh = (byte)gtEl.GetInt32();
            }

            var dienThoai = data.TryGetProperty("dienThoai", out var dtEl)
                ? (dtEl.GetString() ?? "")
                : "";

            var email = data.TryGetProperty("email", out var emEl)
                ? (emEl.GetString() ?? "")
                : "";

            var diaChi = data.TryGetProperty("diaChi", out var dcEl)
                ? (dcEl.GetString() ?? "")
                : "";

            var ghiChu = data.TryGetProperty("ghiChu", out var gcEl)
                ? (gcEl.GetString() ?? "")
                : "";

            return new DM_KhachHang
            {
                KhachHangId = id,
                MaKhachHang = ma,       // Insert: repo sẽ tự sinh mã mới, Update: trường này bị bỏ qua trong UPDATE
                HoTen = hoTen,
                NamSinh = namSinh,
                GioiTinh = gioiTinh,
                DienThoai = dienThoai,
                Email = email,
                DiaChi = diaChi,
                GhiChu = ghiChu
            };
        }

        private async Task HandleCreateKhachAsync(JsonElement data)
        {
            var kh = ParseKhachFromJson(data);
            _khachRepository.Insert(kh);
            await HandleGetKhachListAsync();
        }

        private async Task HandleUpdateKhachAsync(JsonElement data)
        {
            var kh = ParseKhachFromJson(data);
            _khachRepository.Update(kh);
            await HandleGetKhachListAsync();
        }

        private async Task HandleDeleteKhachAsync(JsonElement data)
        {
            var id = data.GetProperty("khachHangId").GetInt32();
            _khachRepository.Delete(id);
            await HandleGetKhachListAsync();
        }
        private async Task HandleGetDonThuocListAsync()
        {
            try
            {
                var list = _donThuocRepository.GetAllForListing();
                await CallJsAsync("onDonThuocList", list);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Lỗi load đơn thuốc");
            }
        }

        private async Task HandleGetDonThuocDetailAsync(JsonElement data)
        {
            try
            {
                var id = data.GetProperty("donThuocId").GetInt32();
                var detail = _donThuocRepository.GetDetail(id);
                await CallJsAsync("onDonThuocDetail", detail);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Lỗi xem đơn thuốc");
            }
        }


        private async Task HandlePasswordResetConfirmAsync(JsonElement data, string jsCallback)
        {
            var userName = data.GetProperty("userName").GetString() ?? string.Empty;
            var code = data.GetProperty("code").GetString() ?? string.Empty;
            var newPass = data.GetProperty("newPassword").GetString() ?? string.Empty;

            var result = _authService.ConfirmPasswordReset(userName, code, newPass);
            await CallJsAsync(jsCallback, result);
        }
        private async Task HandleGetKhachListAsync()
        {
            try
            {
                var list = _khachRepository.GetAll();

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(list, options);
                var script = $"window.App && App.onKhachList({json});";
                await webView21.CoreWebView2.ExecuteScriptAsync(script);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Lỗi load khách hàng");
            }
        }
        private async Task HandleCreateThuocAsync(JsonElement data)
        {
            try
            {
                var maThuoc = "";
                var tenThuoc = data.GetProperty("tenThuoc").GetString() ?? "";
                var tenKhac = data.GetProperty("tenKhac").GetString() ?? "";
                var donViTinh = data.GetProperty("donViTinh").GetString() ?? "";

                var giaBanLe = (decimal)(data.TryGetProperty("giaBanLe", out var giaEl)
                                            ? giaEl.GetDouble()
                                            : 0);
                var tonToiThieu = (decimal)(data.TryGetProperty("tonToiThieu", out var tonEl)
                                            ? tonEl.GetDouble()
                                            : 0);

                var congDung = data.GetProperty("congDung").GetString() ?? "";
                var chongChiDinh = data.GetProperty("chongChiDinh").GetString() ?? "";
                var ghiChu = data.GetProperty("ghiChu").GetString() ?? "";
                if (giaBanLe < 0)
                    throw new Exception("Giá bán lẻ không được âm.");

                if (tonToiThieu < 0)
                    throw new Exception("Tồn tối thiểu không được âm.");
                var thuoc = new Thuoc
                {
                    MaThuoc = maThuoc,
                    TenThuoc = tenThuoc,
                    TenKhac = tenKhac,
                    DonViTinh = donViTinh,
                    GiaBanLe = giaBanLe,
                    TonToiThieu = tonToiThieu,
                    CongDung = congDung,
                    ChongChiDinh = chongChiDinh,
                    GhiChu = ghiChu
                };

                _thuocService.Add(thuoc);

                // reload danh sách
                await HandleGetThuocListAsync();

                // báo về JS
                await CallJsAsync("onThuocSaved", new
                {
                    success = true,
                    message = "Đã thêm thuốc mới thành công."
                });
            }
            catch (Exception ex)
            {
                await CallJsAsync("onThuocSaved", new
                {
                    success = false,
                    message = "Lỗi thêm thuốc: " + ex.Message
                });
            }
        }
        private async Task HandleDeleteThuocAsync(JsonElement data)
        {
            // Lấy id thuốc từ JS
            var thuocId = data.GetProperty("thuocId").GetInt32();

            // Gọi service xóa (thực chất là SoftDelete: IsActive = 0)
            _thuocService.Delete(thuocId);

            // Sau khi xóa, load lại danh sách thuốc và bắn ngược lên JS
            await HandleGetThuocListAsync();
        }
        private async Task HandlePrintDonThuocAsync(int donThuocId)
        {
            try
            {
                var detail = _donThuocRepository.GetDetail(donThuocId);
                if (detail == null)
                {
                    await CallJsAsync("onPrintDonThuocResult", new
                    {
                        success = false,
                        message = "Không tìm thấy đơn thuốc."
                    });
                    return;
                }

                var doc = new DonThuocPdfDocument(detail);

                // Tạo file PDF tạm trong Documents
                var fileName = $"DonThuoc_{detail.SoDon}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var path = System.IO.Path.Combine(folder, fileName);

                var pdfBytes = doc.GeneratePdf();   // hàm mở rộng của QuestPDF
                System.IO.File.WriteAllBytes(path, pdfBytes);

                // Mở file bằng PDF viewer mặc định của Windows
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });

                await CallJsAsync("onPrintDonThuocResult", new
                {
                    success = true,
                    filePath = path
                });
            }
            catch (Exception ex)
            {
                await CallJsAsync("onPrintDonThuocResult", new
                {
                    success = false,
                    message = "Lỗi in đơn thuốc: " + ex.Message
                });
            }
        }
        private async Task HandleUpdateThuocAsync(JsonElement data)
        {
            try
            {
                var thuocId = data.GetProperty("thuocId").GetInt32();
                var maThuoc = "";
                var tenThuoc = data.GetProperty("tenThuoc").GetString() ?? "";
                var tenKhac = data.GetProperty("tenKhac").GetString() ?? "";
                var donViTinh = data.GetProperty("donViTinh").GetString() ?? "";

                var giaBanLe = (decimal)(data.TryGetProperty("giaBanLe", out var giaEl)
                                            ? giaEl.GetDouble()
                                            : 0);
                var tonToiThieu = (decimal)(data.TryGetProperty("tonToiThieu", out var tonEl)
                                            ? tonEl.GetDouble()
                                            : 0);
                // sau khi đọc giaBanLe, tonToiThieu
                if (giaBanLe < 0)
                    throw new Exception("Giá bán lẻ không được âm.");

                if (tonToiThieu < 0)
                    throw new Exception("Tồn tối thiểu không được âm.");

                var congDung = data.GetProperty("congDung").GetString() ?? "";
                var chongChiDinh = data.GetProperty("chongChiDinh").GetString() ?? "";
                var ghiChu = data.GetProperty("ghiChu").GetString() ?? "";

                var thuoc = new Thuoc
                {
                    ThuocId = thuocId,
                    MaThuoc = maThuoc,
                    TenThuoc = tenThuoc,
                    TenKhac = tenKhac,
                    DonViTinh = donViTinh,
                    GiaBanLe = giaBanLe,
                    TonToiThieu = tonToiThieu,
                    CongDung = congDung,
                    ChongChiDinh = chongChiDinh,
                    GhiChu = ghiChu
                };

                _thuocService.Update(thuoc);

                await HandleGetThuocListAsync();

                await CallJsAsync("onThuocSaved", new
                {
                    success = true,
                    message = "Đã cập nhật thuốc thành công."
                });
            }
            catch (Exception ex)
            {
                await CallJsAsync("onThuocSaved", new
                {
                    success = false,
                    message = "Lỗi cập nhật thuốc: " + ex.Message
                });
            }
        }




        private async Task HandleLoginAsync(JsonElement data)
        {
            var userName = data.GetProperty("userName").GetString() ?? string.Empty;
            var password = data.GetProperty("password").GetString() ?? string.Empty;

            var result = _authService.Login(userName, password);

            // 🔥 nếu đăng nhập OK thì lưu UserId hiện tại
            if (result.Success && result.UserId.HasValue)
            {
                _currentUserId = result.UserId.Value;
            }
            else
            {
                _currentUserId = 0;   // hoặc giữ nguyên, tuỳ bạn
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var resultJson = JsonSerializer.Serialize(result, options);
            var script = $"window.App && App.onLoginResult({resultJson});";
            await webView21.CoreWebView2.ExecuteScriptAsync(script);
        }
        private async Task HandleGetThuocListAsync()
        {
            var list = _thuocService.GetAll();

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var resultJson = JsonSerializer.Serialize(list, options);
            var script = $"window.App && App.onThuocList({resultJson});";
            await webView21.CoreWebView2.ExecuteScriptAsync(script);
        }
        private async Task HandleGetUserListAsync()
        {
            var users = _userRepository.GetAllWithRoleName(); // SELECT u.*, r.RoleName...
            var json = JsonSerializer.Serialize(
                users,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            await webView21.CoreWebView2.ExecuteScriptAsync(
                $"window.App && App.onUserList({json});");
        }


        private async Task HandleUpdateUserAsync(JsonElement data)
        {
            // Lấy dữ liệu từ JS gửi qua
            var userId = data.GetProperty("userId").GetInt32();
            var fullName = data.GetProperty("fullName").GetString() ?? string.Empty;
            var email = data.GetProperty("email").GetString() ?? string.Empty;
            var phone = data.GetProperty("phoneNumber").GetString() ?? string.Empty;

            // CHÚ Ý: phía JS phải gửi roleId (int), không phải roleName
            var roleId = data.GetProperty("roleId").GetInt32();

            string? newPassword = null;
            if (data.TryGetProperty("newPassword", out var pwEl))
            {
                var tmp = pwEl.GetString();
                if (!string.IsNullOrWhiteSpace(tmp))
                {
                    newPassword = tmp;
                }
            }

            // 1) Cập nhật họ tên / email / sđt / vai trò
            _authService.UpdateUserBasicInfoAndRole(userId, fullName, email, phone, roleId);

            // 2) Nếu có nhập mật khẩu mới thì đổi mật khẩu
            if (!string.IsNullOrEmpty(newPassword))
            {
                _authService.ChangeUserPassword(userId, newPassword);
            }

            // 3) Load lại danh sách tài khoản để cập nhật UI
            await HandleGetUserListAsync();
        }

        private async Task HandleCreateUserAsync(JsonElement data)
        {
            var userName = data.GetProperty("userName").GetString() ?? "";
            var fullName = data.GetProperty("fullName").GetString() ?? "";
            var email = data.GetProperty("email").GetString() ?? "";
            var phone = data.GetProperty("phoneNumber").GetString() ?? "";

            // LẤY ROLEID (int) THAY VÌ ROLENAME
            var roleId = data.GetProperty("roleId").GetInt32();

            var password = data.GetProperty("password").GetString() ?? "";

            _authService.CreateUser(userName, fullName, email, phone, roleId, password);

            // reload lại danh sách tài khoản trả về cho JS
            await HandleGetUserListAsync();
        }
        private async Task HandleSetUserActiveAsync(JsonElement data)
        {
            int userId = data.GetProperty("userId").GetInt32();
            bool isActive = data.GetProperty("isActive").GetBoolean();

            _userRepository.SetActive(userId, isActive);
            await HandleGetUserListAsync();
        }

        private async Task HandleDeleteUserAsync(JsonElement data)
        {
            int userId = data.GetProperty("userId").GetInt32();
            _userRepository.Delete(userId);
            await HandleGetUserListAsync();
        }
    }

}

