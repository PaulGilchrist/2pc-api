using System.Text;
using API.Models;

namespace API.Services {
    public class MessageServiceDapr: IMessageService {
        private readonly ApplicationSettings _applicationSettings;

        public MessageServiceDapr(ApplicationSettings applicationSettings) {
            _applicationSettings = applicationSettings;
        }

        public void Send(string message) {
            var url = "http://localhost:3500/v1.0/publish/pubsub/" + _applicationSettings.QueueName;
            using var client = new HttpClient();
            var data = new StringContent(message,Encoding.UTF8,"application/json");
            var result = client.PostAsync(url,data).GetAwaiter().GetResult();
            Console.WriteLine(" [x] Sent {0}",message);
        }


    }


}
