using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Mastonet;
using Mastonet.Entities;

namespace MastodonID
{
    /*
    * Retrieve user ids from an instance
    * Uses getDirectory method
    * based on time (set up to 8 hours)
    */
    class Program
    {
        static async Task Main(string[] args)
        {
            string instance = "mastodon.online";

            var clientKey = "E-eLJuqC1ZMBww_HClxA5igWJYkGUXUQChdZP6-H4zw";
            var clientSecret = "FXCFEr5-fZpH91mERzOvw3iCUo4JnmSl7OQTmDc1H5Q";
            var accessToken = "5T1a3vDxBIOjF_AdF6svrwvelv93iLs2mJ2HNkZR-hE";

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

            var userIDs = new HashSet<long>();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int offset = 0;
            const int limit = 80;

            TimeSpan targetDuration = TimeSpan.FromHours(8);
            TimeSpan elapsedDuration;

            while (true)
            {
                try
                {
                    var directoryAccounts = await client.GetDirectory(offset, limit, DirectoryOrder.Active, true);

                    foreach (var account in directoryAccounts)
                    {
                        long userId = long.Parse(account.Id);

                        if (!userIDs.Contains(userId))
                        {
                            userIDs.Add(userId);

                            var followers = await Retry(() => client.GetAccountFollowers(userId.ToString()), TimeSpan.FromSeconds(60), 7);
                            var following = await Retry(() => client.GetAccountFollowing(userId.ToString()), TimeSpan.FromSeconds(60), 7);

                            foreach (var user in followers.Concat(following))
                            {
                                if (!userIDs.Contains(long.Parse(user.Id)) && user.ProfileUrl.Contains(instance))
                                {
                                    userIDs.Add(long.Parse(user.Id));
                                }
                            }

                            Console.WriteLine($"Reached {userIDs.Count} users so far");

                            elapsedDuration = stopwatch.Elapsed;
                            if (elapsedDuration >= targetDuration)
                                break;
                        }
                    }

                    offset += limit;

                    elapsedDuration = stopwatch.Elapsed;
                    if (elapsedDuration >= targetDuration)
                        break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }

            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Console.WriteLine("RunTime to gather user IDS is " + elapsedTime);

            using (var file = new StreamWriter("MOnlineID-8hours.txt"))
            {
                file.WriteLine($"Nr of users from {instance} is {userIDs.Count()}");
                foreach (var userId in userIDs)
                {
                    file.WriteLine(userId);
                }
            }
            Console.WriteLine("User IDs saved to MOnlineID-8hours.txt");
        }

        /*
        Function to retry the API request method.
        Exception error handling for server error (limit of requests for Mastodon reached)
        and Network error (if the network of my computer is not working, so that the program will not stop automatically)
        Mastodon has a limit of 300 requests for every 5 minutes.
        */
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
