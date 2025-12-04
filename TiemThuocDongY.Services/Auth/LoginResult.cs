namespace TiemThuocDongY.Services.Auth
{
    public class LoginResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int? UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string RoleName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;
    }
}
