using System.Diagnostics;
using System.Transactions;
using API.Models;
using Azure.Messaging.ServiceBus;

/*
 * Azure Service Bus Transactions
 * Requires either Standard or Premium tiers (not Basic)
 * https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-transactions
 * https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/servicebus/Azure.Messaging.ServiceBus/samples/Sample06_Transactions.md#transactions-across-entities
 */

namespace API.Services {
    // Example for using multiple different backend message services through a common interface
    public class MessageServiceAzureServiceBus: IMessageService, IDisposable {
        private readonly ApplicationSettings _applicationSettings;
        private ServiceBusSender _sender;
        private ServiceBusReceiver _receiver;
        private TransactionScope? _transactionScope = null;

        private bool disposedValue;

        public MessageServiceAzureServiceBus(ApplicationSettings applicationSettings) {
            _applicationSettings = applicationSettings;
            // Create a ServiceBusClient that will authenticate using a connection string
            string connectionString = _applicationSettings.QueueConnectionString;
            string queueName = _applicationSettings.QueueName;
            // since ServiceBusClient implements IAsyncDisposable we create it with "await using"
            var options = new ServiceBusClientOptions { EnableCrossEntityTransactions = true };
            var client = new ServiceBusClient(connectionString,options);
            // Create the sender and receiver
            _sender = client.CreateSender(queueName);
            _receiver = client.CreateReceiver(queueName);
        }

        public void BeginTransaction() {
            _transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        }

        public void CommitTransaction() {
            _transactionScope?.Complete();
        }

        public void RollbackTransaction() {
            _transactionScope?.Dispose();
            Activity.Current?.AddEvent(new ActivityEvent("AzureServiceBus.Message.Rollback"));
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
            // send the message
            _sender.SendMessageAsync(new ServiceBusMessage(message)).GetAwaiter();
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
                    _transactionScope?.Dispose();
                    _sender.CloseAsync().GetAwaiter();
                    _sender.DisposeAsync().GetAwaiter();
                    _receiver.CloseAsync().GetAwaiter();
                    _receiver.DisposeAsync().GetAwaiter();
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
