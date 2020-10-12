using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VeracodeMessageQueue.MessagingService;

namespace VeracodeMessageQueue
{
    public interface IMessageService
    {
        void SendMessage(MessageTypes type, string message);
    }

    public class MessageService : IMessageService
    {
        private EventGridConfiguration _config;

        public MessageService(IOptions<EventGridConfiguration> options)
        {
            _config = options.Value;
        }

        public void SendMessage(MessageTypes type, string message)
        {
            switch (type)
            {
                case MessageTypes.AppEvent:
                    _ = SendMessagesAsync("AppEvent",  message);
                    break;

                case MessageTypes.BuildEvent:
                    _ = SendMessagesAsync("BuildEvent", message);
                    break;

                case MessageTypes.MitigationEvent:
                    _ = SendMessagesAsync("MitigationEvent", message);
                    break;

                case MessageTypes.FlawEvent:
                    _ = SendMessagesAsync("FlawEvent", message);
                    break;
            }
        }
        private async Task SendMessagesAsync(string eventType, string message)
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("aeg-sas-key", _config.SharedAccessKey);

                var gridEvent = new GridEvent
                {
                    Subject = message,
                    EventType = eventType,
                    EventTime = DateTime.UtcNow,
                    Id = Guid.NewGuid().ToString()
                };

                string json = JsonConvert.SerializeObject(gridEvent);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _config.VeracodeTopicEndpoint)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                HttpResponseMessage response = await client.SendAsync(request);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }
        }
    }
}
