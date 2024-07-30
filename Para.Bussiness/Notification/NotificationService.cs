using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Para.Base.Email;

namespace Para.Bussiness.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly SmtpSettings _smtpSettings;

        public NotificationService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendEmail(string to, string subject, string body)
        {
            var fromAddress = new MailAddress(_smtpSettings.FromAddress, _smtpSettings.FromName);
            var toAddress = new MailAddress(to);
            var smtpClient = new SmtpClient
            {
                Host = _smtpSettings.Host,
                Port = _smtpSettings.Port,
                EnableSsl = _smtpSettings.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, _smtpSettings.FromPassword)
            };

            using (var message = new MailMessage(fromAddress, toAddress)
                   {
                       Subject = subject,
                       Body = body
                   })
            {
                await smtpClient.SendMailAsync(message);
            }
        }
    }
}