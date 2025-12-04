using System;
using System.Collections.Generic;
using TiemThuocDongY.Data.Repositories;
using TiemThuocDongY.Domain.Entities;
using TiemThuocDongY.Services.Email;

namespace TiemThuocDongY.Services.Auth
{
    public class AuthService
    {
        private readonly UserRepository _userRepository;
        private readonly PasswordResetRepository _resetRepository;
        private readonly EmailService _emailService;

        public AuthService(UserRepository userRepository, EmailService emailService)
        {
            _userRepository = userRepository;
            _resetRepository = new PasswordResetRepository();
            _emailService = emailService;
        }

        // ============== ĐĂNG NHẬP ==============
        public LoginResult Login(string userName, string password)
        {
            var user = _userRepository.GetByUserName(userName);
            if (user == null || !user.IsActive)
            {
                return new LoginResult
                {
                    Success = false,
                    Message = "Tài khoản không tồn tại hoặc đã bị khóa."
                };
            }

            var hashedInput = HashPassword(password);

            if (!string.Equals(user.PasswordHash, hashedInput, StringComparison.Ordinal))
            {
                return new LoginResult
                {
                    Success = false,
                    Message = "Mật khẩu không đúng."
                };
            }

            return new LoginResult
            {
                Success = true,
                Message = "Đăng nhập thành công.",
                UserId = user.UserId,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                RoleName = user.RoleName
            };
        }

        // TẠM THỜI: trả lại plain text để khớp DB hiện tại.
        // Sau này muốn bảo mật hơn chỉ cần đổi implement.
        private string HashPassword(string password)
        {
            return password ?? string.Empty;
        }

        // ============== DÙNG CHO MÀN HÌNH TÀI KHOẢN ==============

        public IList<SysUser> GetAllUsers()
        {
            return new List<SysUser>(_userRepository.GetAllWithRoleName());
        }

        // Sửa họ tên, email, điện thoại, vai trò
        public void UpdateUserBasicInfoAndRole(
            int userId,
            string fullName,
            string email,
            string phoneNumber,
            int roleId)
        {
            _userRepository.UpdateBasicInfoAndRole(userId, fullName, email, phoneNumber, roleId);
        }

        // Khóa / mở tài khoản
        public void SetUserActive(int userId, bool isActive)
        {
            _userRepository.SetActive(userId, isActive);
        }

        // Xóa tài khoản
        public void DeleteUser(int userId)
        {
            _userRepository.Delete(userId);
        }

        // ============== RESET MẬT KHẨU (QUÊN MẬT KHẨU) ==============

        private static string GenerateCode()
        {
            // mã 6 chữ số: 100000–999999
            var rnd = new Random();
            var value = rnd.Next(100000, 1000000);   // max exclusive
            return value.ToString("D6");
        }

        public SimpleResult RequestPasswordReset(string userNameOrEmail)
        {
            var user = _userRepository.GetByUserNameOrEmail(userNameOrEmail);
            if (user == null)
            {
                return new SimpleResult
                {
                    Success = false,
                    Message = "Tài khoản không tồn tại."
                };
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return new SimpleResult
                {
                    Success = false,
                    Message = "Tài khoản này chưa có email, không thể gửi mã."
                };
            }

            var code = GenerateCode();
            var expires = DateTime.Now.AddMinutes(10);

            _resetRepository.Insert(user.UserId, code, expires);

            // Nếu chưa cấu hình EmailService thì vẫn cho dùng DEMO
            if (_emailService == null)
            {
                return new SimpleResult
                {
                    Success = true,
                    Message = $"Đã tạo mã đặt lại mật khẩu.\n(DEMO: mã là {code})"
                };
            }

            var subject = "Mã đặt lại mật khẩu - Tiệm thuốc Đông y";
            var displayName = string.IsNullOrWhiteSpace(user.FullName)
                ? user.UserName
                : user.FullName;

            var body = $@"
        <p>Xin chào {System.Net.WebUtility.HtmlEncode(displayName)},</p>
        <p>Mã đặt lại mật khẩu của bạn là: <strong>{code}</strong></p>
        <p>Mã có hiệu lực đến: {expires:dd/MM/yyyy HH:mm}</p>
        <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
    ";

            try
            {
                _emailService.Send(user.Email, subject, body);

                return new SimpleResult
                {
                    Success = true,
                    Message = $"Đã gửi mã đặt lại mật khẩu tới email {user.Email}."
                };
            }
            catch (Exception ex)
            {
                // dev: cho xem ex.Message; sau này prod có thể ẩn đi
                return new SimpleResult
                {
                    Success = false,
                    Message = "Không gửi được email. Vui lòng thử lại sau.\n" + ex.Message
                };
            }
        }


        public SimpleResult ConfirmPasswordReset(string userNameOrEmail, string code, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                return new SimpleResult
                {
                    Success = false,
                    Message = "Mật khẩu mới không được để trống."
                };
            }

            var user = _userRepository.GetByUserNameOrEmail(userNameOrEmail);
            if (user == null)
            {
                return new SimpleResult
                {
                    Success = false,
                    Message = "Tài khoản không tồn tại."
                };
            }

            var record = _resetRepository.GetValidByUserAndCode(user.UserId, code);
            if (record == null)
            {
                return new SimpleResult
                {
                    Success = false,
                    Message = "Mã xác nhận không hợp lệ hoặc đã hết hạn."
                };
            }

            var hash = HashPassword(newPassword);
            _userRepository.UpdatePassword(user.UserId, hash);
            _resetRepository.MarkUsed(record.ResetId);

            return new SimpleResult
            {
                Success = true,
                Message = "Đổi mật khẩu thành công."
            };
        }

        // ============== ADMIN TẠO / ĐỔI MK USER ==============

        // Thêm tài khoản mới (Admin)
        public int CreateUser(
            string userName,
            string fullName,
            string email,
            string phoneNumber,
            int roleId,
            string password)
        {
            var user = new SysUser
            {
                UserName = userName,
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                RoleId = roleId,
                IsActive = true,
                IsEmailConfirmed = false,
                PasswordHash = HashPassword(password)
            };

            return _userRepository.Insert(user);
        }

        // Admin đổi mật khẩu cho tài khoản khác
        public void ChangeUserPassword(int userId, string newPassword)
        {
            var hash = HashPassword(newPassword);
            _userRepository.UpdatePassword(userId, hash);
        }
    }
}
