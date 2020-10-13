using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace VeracodeMQLog
{
    public static class LogEvent
    {
        [FunctionName("LogEvent")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<IEnumerable<GridEvent<object>>>(requestBody);
            foreach (var item in data)
            {
                if (item.EventType.Equals("Microsoft.EventGrid.SubscriptionValidationEvent"))
                {
                    dynamic webhookAuth = item.Data;
                    return new OkObjectResult(new { validationResponse = webhookAuth.validationCode });
                }
                log.LogInformation($"Event => {item.EventType} Subject => {item.Subject}\n");
            }

            return new OkResult();
        }
    }
}
