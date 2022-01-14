using API.Models;
using Azure.Messaging.ServiceBus;

namespace API.Services {
    // Example for using multiple different backend message services through a common interface
    public class MessageServiceAzureServiceBus: IMessageService {
        private readonly ApplicationSettings _applicationSettings;
        ServiceBusSender _sender;
        ServiceBusReceiver _receiver;

        public MessageServiceAzureServiceBus(ApplicationSettings applicationSettings) {
            _applicationSettings = applicationSettings;
            // Create a ServiceBusClient that will authenticate using a connection string
            string connectionString = _applicationSettings.QueueConnectionString;
            string queueName = _applicationSettings.QueueName;
            // since ServiceBusClient implements IAsyncDisposable we create it with "await using"
            var client = new ServiceBusClient(connectionString);
            // Create the sender and receiver
            _sender = client.CreateSender(queueName);
            _receiver = client.CreateReceiver(queueName);
        }

        public void Send(string message) {
            // create a message that we can send. UTF-8 encoding is used when providing a string.
            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(message);
            // send the message
            _sender.SendMessageAsync(serviceBusMessage).GetAwaiter().GetResult();
            Console.WriteLine(" [x] Sent {0}",message);
        }

        public string Receive() {
            // the received message is a different type as it contains some service set properties
            ServiceBusReceivedMessage receivedMessage = _receiver.ReceiveMessageAsync().GetAwaiter().GetResult();
            // get the message body as a string
            return receivedMessage.Body.ToString();
        }
    }
}
