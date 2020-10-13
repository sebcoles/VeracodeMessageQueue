using DocumentFormat.OpenXml.Drawing.Charts;
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
        void SendMessage(MessageTypes type, string message, object entity);
    }

    public class MessageService : IMessageService
    {
        private EventGridConfiguration _config;

        public MessageService(IOptions<EventGridConfiguration> options)
        {
            _config = options.Value;
        }

        public void SendMessage(MessageTypes type, string message, object entity)
        {
            switch (type)
            {
                case MessageTypes.AppEvent:
                    SendMessages("AppEvent",  message, entity);
                    break;

                case MessageTypes.BuildEvent:
                    SendMessages("BuildEvent", message, entity);
                    break;

                case MessageTypes.MitigationEvent:
                    SendMessages("MitigationEvent", message, entity);
                    break;

                case MessageTypes.FlawEvent:
                    SendMessages("FlawEvent", message, entity);
                    break;
            }
        }
        private HttpResponseMessage SendMessages(string eventType, string message, object entity)
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("aeg-sas-key", _config.SharedAccessKey);

                var list = new List<GridEvent<object>>()
                { new GridEvent<object>
                {
                    Subject = message,
                    EventType = eventType,
                    EventTime = DateTime.UtcNow,
                    Id = Guid.NewGuid().ToString(),
                    Data = entity
                }};

                string json = JsonConvert.SerializeObject(list);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _config.VeracodeTopicEndpoint)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                var response = client.SendAsync(request).Result;
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new Exception(response.ReasonPhrase);

                return response;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
                throw exception;
            }
        }
    }
}
