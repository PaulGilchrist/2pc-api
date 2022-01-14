using System.Diagnostics;
using API.Models;
using Azure.Messaging.ServiceBus;

namespace API.Services {
    // Example for using multiple different backend message services through a common interface
    public class MessageServiceAzureServiceBus: IMessageService, IDisposable {
        private readonly ApplicationSettings _applicationSettings;
        ServiceBusSender _sender;
        ServiceBusReceiver _receiver;
        private bool disposedValue;

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

        public void BeginTransaction() {
            Activity.Current?.AddEvent(new ActivityEvent("AzureServiceBus.Message.BeginTransaction Not Implemented"));
        }

        public void CommitTransaction() {
            Activity.Current?.AddEvent(new ActivityEvent("AzureServiceBus.Message.CommitTransaction Not Implemented"));
        }

        public void RollbackTransaction() {
            Activity.Current?.AddEvent(new ActivityEvent("AzureServiceBus.Message.RollbackTransaction Not Implemented"));
        }

        public string? Receive() {
            // the received message is a different type as it contains some service set properties
            ServiceBusReceivedMessage receivedMessage = _receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
            // get the message body as a string
            string? message = null;
            if(receivedMessage != null) {
                message = receivedMessage.Body.ToString();
                var activityTagsCollection = new ActivityTagsCollection();
                activityTagsCollection.Add("message",message);
                Activity.Current?.AddEvent(new ActivityEvent("AzureServiceBus.Message.Received",default,activityTagsCollection));
            }
            return message;
        }

        public void Send(string message) {
            // create a message that we can send. UTF-8 encoding is used when providing a string.
            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(message);
            // send the message
            _sender.SendMessageAsync(serviceBusMessage).GetAwaiter().GetResult();
            var activityTagsCollection = new ActivityTagsCollection();
            activityTagsCollection.Add("message",message);
            Activity.Current?.AddEvent(new ActivityEvent("AzureServiceBus.Message.Sent",default,activityTagsCollection));
        }

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if(!disposedValue) {
                if(disposing) {
                    // TODO: dispose managed state (managed objects)
                    _sender.CloseAsync().GetAwaiter();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~MessageServiceAzureServiceBus()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

    }
}
