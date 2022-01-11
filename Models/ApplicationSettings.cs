using System;

namespace API.Models {
    public class ApplicationSettings {
        public string ContactsCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string QueueHostName { get; set; }
        public ApplicationSettings() {
            ConnectionString = Environment.GetEnvironmentVariable("ConnectionString");
            DatabaseName = Environment.GetEnvironmentVariable("DatabaseName");
            ContactsCollectionName = Environment.GetEnvironmentVariable("ContactsCollectionName");
            QueueHostName = Environment.GetEnvironmentVariable("QueueHostName");
        }
    }
}
