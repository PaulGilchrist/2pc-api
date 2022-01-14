using System;

namespace API.Models {
    public class ApplicationSettings {
        public string ContactsCollectionName { get; set; }
        public string DatabaseConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string QueueConnectionString { get; set; }
        public string QueueName { get; set; }
        public string QueueType { get; set; }
        public string TelemetryConnectionString { get; set; }
        public string TelemetryType { get; set; }
        public ApplicationSettings() {
            DatabaseConnectionString = Environment.GetEnvironmentVariable("DatabaseConnectionString");
            DatabaseName = Environment.GetEnvironmentVariable("DatabaseName");
            ContactsCollectionName = Environment.GetEnvironmentVariable("ContactsCollectionName");
            QueueConnectionString = Environment.GetEnvironmentVariable("QueueConnectionString"); // Same as HostName for QueueType="RabbitMQ"
            QueueName = Environment.GetEnvironmentVariable("QueueName");
            QueueType = Environment.GetEnvironmentVariable("QueueType"); // Valid options are "AzureServiceBus", "Dapr", "None", or "RabbitMQ" (Default: "None")
            TelemetryConnectionString = Environment.GetEnvironmentVariable("TelemetryConnectionString"); // Blank if TelemetryType="Console"
            TelemetryType = Environment.GetEnvironmentVariable("TelemetryType"); // Valid options are "AppInsights", "Console", or "Zipkin" (Default: "Console")
        }
    }
}
