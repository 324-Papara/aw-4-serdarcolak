using System.Text;
using Newtonsoft.Json;
using Para.Bussiness.Notification;
using Para.Bussiness.RabbitMq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Para.Bussiness.Job
{
    public class EmailProcessorJob
    {
        private readonly INotificationService notificationService;
        private readonly IRabbitMQService rabbitMQService;

        public EmailProcessorJob(INotificationService notificationService, IRabbitMQService rabbitMQService)
        {
            this.notificationService = notificationService;
            this.rabbitMQService = rabbitMQService;
        }

        public void ProcessEmailQueue()
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "username",
                Password = "password"
            };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var emailMessage = JsonConvert.DeserializeObject<EmailMessage>(message);

                    notificationService.SendEmail(emailMessage.Email, emailMessage.Subject, emailMessage.Body).Wait();
                };

                channel.BasicConsume(queue: "emailQueue", autoAck: true, consumer: consumer);
            }
        }
    }

    public class EmailMessage
    {
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}