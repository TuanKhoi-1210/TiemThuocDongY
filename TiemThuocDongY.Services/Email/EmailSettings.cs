namespace TiemThuocDongY.Services.Email
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = "";
        public int SmtpPort { get; set; } = 587;
        public string UserName { get; set; } = "";        // tài khoản SMTP (vd: gmail)
        public string Password { get; set; } = "";        // app password
        public bool EnableSsl { get; set; } = true;

        public string FromAddress { get; set; } = "";     // địa chỉ From (thường = UserName)
        public string FromDisplayName { get; set; } = "Tiệm Thuốc Đông Y";
    }
}
