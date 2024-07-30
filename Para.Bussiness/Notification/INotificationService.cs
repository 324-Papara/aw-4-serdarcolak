using System.Threading.Tasks;

namespace Para.Bussiness.Notification
{
    public interface INotificationService
    {
        Task SendEmail(string to, string subject, string body);
    }
}