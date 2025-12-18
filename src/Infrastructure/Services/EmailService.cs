// using System.Net.Mail;
// using MailKit.Net.Smtp;
// using Microsoft.Extensions.Configuration;
// using MimeKit;
namespace Infrastructure.Services;
//public interface IEmailService { Task SendAsync(string to, string subject, string html); }

//public class EmailService : IEmailService
//{
    // private readonly IConfiguration _cfg;
    // public EmailService(IConfiguration cfg) => _cfg = cfg;
    // public async Task SendAsync(string to, string subject, string html)
    // {
    //     var msg = new MimeMessage();
    //     msg.From.Add(new MailboxAddress(_cfg["Email:FromName"] ?? "Store", _cfg["Email:From"] ?? "no-reply@example.com"));
    //     msg.To.Add(MailboxAddress.Parse(to));
    //     msg.Subject = subject;
    //     msg.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = html };
    //     using var smtp = new SmtpClient();
    //     await smtp.ConnectAsync(_cfg["Email:SmtpHost"], int.Parse(_cfg["Email:SmtpPort"] ?? "25"), false);
    //     if (!string.IsNullOrEmpty(_cfg["Email:SmtpUser"])) await smtp.AuthenticateAsync(_cfg["Email:SmtpUser"], _cfg["Email:SmtpPass"]);
    //     await smtp.SendAsync(msg);
    //     await smtp.DisconnectAsync(true);
    // }
//}
