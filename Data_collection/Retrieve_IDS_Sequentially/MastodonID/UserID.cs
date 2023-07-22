using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MastodonID
{
    public class UserID
    {
        private readonly HttpClient _httpClient;
        private readonly string _mastodonInstanceUrl;

        public UserID(string mastodonInstanceUrl)
        {
            _httpClient = new HttpClient();
            _mastodonInstanceUrl = mastodonInstanceUrl;
        }

        public async Task<bool> IsUserIdRegisteredAsync(string userId)
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"{_mastodonInstanceUrl}/api/v1/accounts/{userId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var accountData = JObject.Parse(content);

                string profileUrl = accountData.Value<string>("url");

                // Check if the user's profile URL starts with the instance URL
                if (profileUrl.StartsWith(_mastodonInstanceUrl))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
