using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Mastonet;
using Mastonet.Entities;
using Newtonsoft.Json;

namespace MastodonID
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string instance = "mastodon.social";
            string filePath = "data18k.txt";

            var clientKey = "-ZO7ciJps2aQnDFz4LH6LmaXJ1AUsA7wucIS5pq6jug";
            var clientSecret = "Ohr0DIidoyQwfqsd1QoloBYkk-xLTcUVgJQUnWo6okY";
            var accessToken = "rp3pn5wt133TbP_hCuRsO3VN3v5WNcTD3sGsL_7tv_g";

            var appRegistration = new AppRegistration
            {
                Instance = instance,
                ClientId = clientKey,
                ClientSecret = clientSecret
            };

            var auth = new Auth
            {
                AccessToken = accessToken
            };

            var client = new MastodonClient(instance, accessToken);

            List<long> userIDs = new List<long>();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            using (StreamReader reader = new StreamReader(filePath)){
                string line;
                
                while ((line = reader.ReadLine()) != null){
                    long userID = long.Parse(line);
                    userIDs.Add(userID);
                }
            }

            int counter = 0;

            using (var file = new StreamWriter("MSocial-18k.txt"))
            {
                file.WriteLine($"Nr of users from {instance} is {userIDs.Count()}");
                foreach (var userId in userIDs)
                {
                    var account = await Retry(() => client.GetAccount(userId.ToString()), TimeSpan.FromSeconds(60), 7);
                    var serializedAccount = JsonConvert.SerializeObject(account);
                    file.WriteLine(serializedAccount);
                    counter++;
                    Console.WriteLine($"Reached {counter} users");
                }
            }
            Console.WriteLine("User IDs saved to MSocial-18k.txt");

            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Console.WriteLine("RunTime to gather user IDS is " + elapsedTime);
        }

        static async Task<T> Retry<T>(Func<Task<T>> func, TimeSpan delay, int maxRetries)
        {
            int retries = 0;
            while (true)
            {
                try
                {
                    return await func();
                }
                catch (ServerErrorException ex)
                {
                    if (ex.Message.Contains("Too many requests"))
                    {
                        // If "Too many requests" error, wait for a longer time before retrying
                        Console.WriteLine("Too many requests, waiting for 60 seconds before retrying");
                        await Task.Delay(TimeSpan.FromSeconds(60));
                        continue;
                    }

                    if (ex.Message.Contains("Record not found"))
                    {
                        Console.WriteLine("Record not found, skipping user");
                        return default(T);
                    }

                    if (retries < maxRetries)
                    {
                        Console.WriteLine($"Server error, retrying in {delay.TotalSeconds} seconds");
                        await Task.Delay(delay);
                        delay = TimeSpan.FromSeconds(delay.TotalSeconds);
                        retries++;
                    }
                    else
                    {
                        Console.WriteLine($"Server error: {ex.Message}");
                        throw;
                    }
                }
                catch (HttpRequestException ex)
                {
                    if (retries < maxRetries)
                    {
                        Console.WriteLine($"Network error, retrying in {delay.TotalSeconds} seconds");
                        await Task.Delay(delay);
                        delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2); // double the delay time
                        retries++;
                    }
                    else
                    {
                        Console.WriteLine($"Network error: {ex.Message}");
                        throw;
                    }
                }
            }
        }
    }
}