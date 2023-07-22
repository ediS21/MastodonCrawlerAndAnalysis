using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Mastonet;
using Mastonet.Entities;
using Newtonsoft.Json;

//Retrieve account information from list of unique users IDs from mastodon.cloud instance
//Aprox 5hours for 18k requests
namespace MastodonID
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string filePath = "MWorld-8k-id.txt";
            string instance = "mastodon.world";

            var clientKey = "MCZpzflb1e7ZjivHctMxHogzbnfKAsJ2DrQ6UPqhFY0";
            var clientSecret = "S-nmdSINjFqoXuaRihgxozpA4hBM5ms_czS5SvvsT_8";
            var accessToken = "ePoF4AOYfYyWZlfuwp6aHO6LLD66YqUpnFKUG-bHrV4";

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

            //Place of user ids in a list, called userIDs
            using (StreamReader reader = new StreamReader(filePath)){
                string line;
                
                while ((line = reader.ReadLine()) != null){
                    long userID = long.Parse(line);
                    userIDs.Add(userID);
                }
            }

            int counter = 0;

            /*
            Get all account information from an account
            */
            using (var file = new StreamWriter("MWorld-8k-data.txt"))
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
            Console.WriteLine("User IDs saved to MWorld-8k-data.txt");

            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Console.WriteLine("RunTime to gather user IDS is " + elapsedTime);
        }

        //Function to delay the program and wait until Mastodon API rate limit cooldown is finished
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
                        //Log exception message and skip user
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
                catch (JsonReaderException ex){
                    // Log exception message and skip element
                    Console.WriteLine($"JSON parse error: {ex.Message}. Skipping current element.");
                    return default(T);
                }
            }
        }
    }
}