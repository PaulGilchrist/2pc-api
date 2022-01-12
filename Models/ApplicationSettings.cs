using System;

namespace API.Models {
    public class ApplicationSettings {
        public string ContactsCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string QueueType { get; set; }
        public string QueueHostName { get; set; }
        public string QueueName { get; set; }
        public ApplicationSettings() {
            ConnectionString = Environment.GetEnvironmentVariable("ConnectionString");
            DatabaseName = Environment.GetEnvironmentVariable("DatabaseName");
            ContactsCollectionName = Environment.GetEnvironmentVariable("ContactsCollectionName");
            QueueType = Environment.GetEnvironmentVariable("QueueType"); // Valid options are "AzureServiceBus", "RabbitMQ", or "Dapr"
            QueueHostName = Environment.GetEnvironmentVariable("QueueHostName");
            QueueName = Environment.GetEnvironmentVariable("QueueName");
        }
    }
}
