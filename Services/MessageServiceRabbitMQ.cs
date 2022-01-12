using System.Text;
using API.Models;
using RabbitMQ.Client;

namespace API.Services {
    public class MessageServiceRabbitMQ: IMessageService {
        private readonly ApplicationSettings _applicationSettings;
        private ConnectionFactory _factory;

        public MessageServiceRabbitMQ(ApplicationSettings applicationSettings) {
            _applicationSettings = applicationSettings;
            _factory = new ConnectionFactory();
            //_factory.UserName = _applicationSettings.QueueUser;
            //_factory.Password = _applicationSettings.QueuePassword;
            //_factory.VirtualHost = _applicationSettings.QueueVHost;
            _factory.HostName = _applicationSettings.QueueHostName;
            //_factory.Port = _applicationSettings.QueuePort;
        }

        public void Send(string message) {
            try {
                using(var connection = _factory.CreateConnection()) {
                    using(var channel = connection.CreateModel()) {
                        // var args = new Dictionary<string, object>();
                        // args.Add("x-message-ttl", 3600000); // 60 minutes
                        // model.QueueDeclare("api", false, false, false, args);
                        channel.QueueDeclare(queue: _applicationSettings.QueueName,
                                                durable: false,
                                                exclusive: false,
                                                autoDelete: false,
                                                arguments: null);
                        var body = Encoding.UTF8.GetBytes(message);
                        channel.BasicPublish(exchange: "",
                                                routingKey: _applicationSettings.QueueName,
                                                basicProperties: null,
                                                body: body);
                    }
                }
            } catch(Exception ex) {
                Console.WriteLine(ex);
            }
            Console.WriteLine(" [x] Sent {0}",message);
        }


    }


}
