using System.Net;
using System.Net.Mail;

namespace TiemThuocDongY.Services.Email
{
    public class EmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(EmailSettings settings)
        {
            _settings = settings;
        }

        public void Send(string toEmail, string subject, string htmlBody)
        {
            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                EnableSsl = _settings.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_settings.UserName, _settings.Password)
            };

            var fromAddress = string.IsNullOrWhiteSpace(_settings.FromAddress)
                ? _settings.UserName
                : _settings.FromAddress;

            var message = new MailMessage
            {
                From = new MailAddress(fromAddress, _settings.FromDisplayName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            client.Send(message);   // sync cho đơn giản
        }
    }
}
