using System;

namespace TiemThuocDongY.Domain.Entities
{
    public class PasswordReset
    {
        public int ResetId { get; set; }
        public int UserId { get; set; }
        public string Code { get; set; } = "";
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
    }
}