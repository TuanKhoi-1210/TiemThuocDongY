namespace TiemThuocDongY.Domain.Entities;

public class SysUser
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsEmailConfirmed { get; set; }
    public bool IsActive { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty; // JOIN từ Sys_Role
}
