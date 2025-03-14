using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using ServerApp.BLL.Services.InterfaceServices;
using ServerApp.BLL.Services.ViewModels;

namespace ServerApp.BLL.Services
{

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            // Lấy giá trị từ IOptions<EmailSettings>
            _emailSettings = emailSettings.Value;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Sender", _emailSettings.FromEmail));
            message.To.Add(new MailboxAddress("Recipient", to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            message.Body = bodyBuilder.ToMessageBody();

            try
            {
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, false);
                    await client.AuthenticateAsync(_emailSettings.FromEmail, _emailSettings.SmtpPassword);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                throw new InvalidOperationException("Email sending failed", ex);
            }
        }
    }


}
