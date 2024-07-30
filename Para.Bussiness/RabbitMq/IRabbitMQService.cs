using Para.Bussiness.Job;

namespace Para.Bussiness.RabbitMq
{
    public interface IRabbitMQService
    {
        Task PublishToQueue(EmailMessage message, string queueName);
    }
}