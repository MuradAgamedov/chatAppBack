using System.Net.Mail;
using System.Net;

namespace chatApp.Services
{
    public class EmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var smtpClient = new SmtpClient("mail.football.biz")
            {
                Port = 587,
                Credentials = new NetworkCredential("info@football.biz", "D$;Lh?.Cq5^?"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("info@football.biz", "Your App Name"),
                Subject = subject,
                Body = message,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);

            try
            {
                await smtpClient.SendMailAsync(mailMessage).ConfigureAwait(false);
                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
            }
        }
    }
}