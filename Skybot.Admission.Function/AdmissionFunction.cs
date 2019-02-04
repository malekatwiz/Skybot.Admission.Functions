using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Skybot.Admission.Function.Models;
using Twilio.AspNet.Core;

namespace Skybot.Admission.Function
{
    public static class AdmissionFunction
    {
        private const string TopicName = "incomingqueries";
        private static readonly TopicClient TopicClient;

        static AdmissionFunction()
        {
            TopicClient = new TopicClient(Settings.ServiceBusConnectionString, TopicName);
        }

        [FunctionName("AdmissionFunc")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest request, ILogger log)
        {
            log.LogInformation($"Incoming request on Admission Func..");

            if (RequestIsAuthorized(request))
            {
                var userAccount = ExtractUserAccount(request);
                if (string.IsNullOrEmpty(userAccount.PhoneNumber))
                {
                    return new BadRequestResult();
                }

                if (await AccountsClient.HasAccount(userAccount.PhoneNumber))
                {
                    log.LogInformation($"Creating topic for existing user: {userAccount.PhoneNumber}");
                    await PushTopic(userAccount, new Dictionary<string, object>
                    {
                        {"state", MessageState.Exists },
                        {"query", GetUserQuery(request) }
                    });
                }
                else
                {
                    log.LogInformation($"Creating topic for new user: {userAccount.PhoneNumber}");
                    await PushTopic(userAccount, new Dictionary<string, object>
                    {
                        {"state", MessageState.New }
                    });
                }

                return new TwiMLResult();
            }

            return new UnauthorizedResult();
        }

        private static UserAccount ExtractUserAccount(HttpRequest request)
        {
            if (request.Form.TryGetValue("From", out var from))
            {
                return new UserAccount
                {
                    PhoneNumber = from
                };
            }

            return new UserAccount();
        }

        private static string GetUserQuery(HttpRequest request)
        {
            if (request.Form.TryGetValue("Body", out var query))
            {
                return query;
            }

            return string.Empty;
        }

        private static bool RequestIsAuthorized(HttpRequest request)
        {
            return Settings.SecretKey.Equals(request.Query["key"], StringComparison.InvariantCulture);
        }

        private static Task PushTopic(UserAccount userAccount, IDictionary<string, object> properties)
        {
            var message = new Message
            {
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(userAccount))
            };

            foreach (var property in properties)
            {
                message.UserProperties.Add(property);
            }

            return TopicClient.SendAsync(message);
        }
    }
}
