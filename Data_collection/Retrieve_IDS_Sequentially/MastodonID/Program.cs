using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace MastodonID
{
    //Retrieve user IDs sequentially
    class Program
    {
        static async Task Main(string[] args)
        {
            string userToken = "rp3pn5wt133TbP_hCuRsO3VN3v5WNcTD3sGsL_7tv_g"; 

            string userID;

            var mastodonInstanceUrl = "https://mastodon.social"; 

            StringBuilder resultsOutput = new StringBuilder();
            string mastodonSocialIDPath = "mastodon-social-IDs20k.txt";

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            int countRequests = 0;
            int nrUsers = 0;

            for (int i = 1 ; i <= 20000 ; i++) {
                userID = i.ToString();

                var checker = new UserID(mastodonInstanceUrl);
                bool isRegistered = await checker.IsUserIdRegisteredAsync(userID);
                countRequests++;
                
                if (isRegistered){
                    nrUsers++;
                    Console.WriteLine($"Accout with userID {userID} is registered on mastodon.social");
                    resultsOutput.AppendLine(userID);
                }

                if (countRequests % 299 == 0){
                    Console.WriteLine($"Current nr of users is {nrUsers}");
                    await Task.Delay(TimeSpan.FromMinutes(5));
                }
            }

            stopwatch.Stop();

            TimeSpan time = stopwatch.Elapsed;
            string timeDuration = String.Format("{0:00}:{1:00}:{2:00}:{3:00}",time.Hours,time.Minutes,time.Seconds,time.Microseconds/10);

            Console.WriteLine($"Time duration is {timeDuration}");

            File.WriteAllText(mastodonSocialIDPath, resultsOutput.ToString());
        }
    }
}
