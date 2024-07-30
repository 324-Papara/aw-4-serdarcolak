using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Para.Bussiness.Job;
using RabbitMQ.Client;

namespace Para.Bussiness.RabbitMq
{
    public class RabbitMQService : IRabbitMQService
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        
        public RabbitMQService()
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "username",
                Password = "password"
            };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            // Kuyruğu oluşturma
            channel.QueueDeclare(queue: "emailQueue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        public async Task PublishToQueue(EmailMessage message, string queueName)
        {
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            // Mesajı kuyruğa gönderme
            channel.BasicPublish(exchange: "",
                routingKey: queueName,
                basicProperties: null,
                body: body);
        }

        public void Dispose()
        {
            channel.Close();
            connection.Close();
        }
    }
}