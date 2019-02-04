using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Skybot.Admission.Function
{
    public static class AccountsClient
    {
        private static readonly HttpClient HttpClient;
        private static string _token;

        static AccountsClient()
        {
            HttpClient = new HttpClient();
        }

        public static async Task<bool> HasAccount(string phoneNumber)
        {
            await CheckToken();

            HttpClient.DefaultRequestHeaders.Clear();
            HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");

            var response = await HttpClient.GetAsync($"{Settings.SkybotAccountsUri}/api/accounts/check/{phoneNumber}");
            return response.StatusCode.Equals(HttpStatusCode.Found);
        }

        private static async Task CheckToken()
        {
            if (string.IsNullOrEmpty(_token))
            {
                await RequestToken();
            }
        }

        private static async Task RequestToken()
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"client_id", Settings.SkybotAuthClientId },
                {"client_secret", Settings.SkybotAuthClientSecret },
                {"grant_type", "client_credentials" }
            });

            var response = await HttpClient.PostAsync($"{Settings.SkybotAuthUri}/connect/token", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            var deserializedContent = JsonConvert.DeserializeObject<dynamic>(responseContent);

            _token = deserializedContent.access_token;
        }
    }
}
