using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace dienthoai.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendOtpAsync(string toEmail, string toName, string otpCode)
        {
            var settings = _config.GetSection("EmailSettings");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(settings["SenderName"], settings["SenderEmail"]));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = "Mã xác thực OTP - Zuzong Store";

            message.Body = new TextPart("html")
            {
                Text = $@"
                    <div style='font-family:sans-serif;max-width:500px;margin:auto'>
                        <h2 style='color:#e74c3c'>Zuzong Store</h2>
                        <p>Xin chào <strong>{toName}</strong>,</p>
                        <p>Mã OTP xác thực tài khoản của bạn là:</p>
                        <div style='font-size:36px;font-weight:bold;letter-spacing:8px;
                                    color:#e74c3c;text-align:center;padding:20px;
                                    background:#f8f8f8;border-radius:8px;margin:16px 0'>
                            {otpCode}
                        </div>
                        <p style='color:#888'>Mã có hiệu lực trong <strong>5 phút</strong>. 
                        Không chia sẻ mã này với bất kỳ ai.</p>
                    </div>"
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(settings["SmtpHost"]!, int.Parse(settings["SmtpPort"]!), 
                SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(settings["SenderEmail"], settings["SenderPassword"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}