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
            string instance = "mastodon.uno";

            var clientKey = "akV9tUUNjd9jav6e26YtmeSFb3ReH34_woHFD3c74sY";
            var clientSecret = "bSIuq43GOOi0PjdnhsWhkDWrr5ucZZnTpWmPrjOkw14";
            var accessToken = "LmvVFlTMfmUGnMbiJwbcn8XgZsUoZt3FHfgnJNt8SGI";

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

            TimeSpan targetDuration = TimeSpan.FromMinutes(10);
            TimeSpan elapsedDuration;

            while (true)
            {
                try
                {
                    // var directoryAccounts = await client.GetDirectory(offset, limit, DirectoryOrder.Active, true);
                    var directoryAccounts = await Retry(async () => await client.GetDirectory(offset, limit, DirectoryOrder.Active, true), TimeSpan.FromSeconds(60), 6);

                    var userTasks = directoryAccounts
                        .Where(account => !userIDs.Contains(long.Parse(account.Id)))
                        .Select(account => GetUserDetailsAsync(client, userIDs, instance, account.Id));

                    var userDetails = await Task.WhenAll(userTasks);

                    foreach (var users in userDetails)
                    {
                        userIDs.UnionWith(users);
                    }

                    Console.WriteLine($"Reached {userIDs.Count} users so far");

                    elapsedDuration = stopwatch.Elapsed;
                    if (elapsedDuration >= targetDuration)
                        break;

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

            using (var file = new StreamWriter("MastodonITALY2.txt"))
            {
                file.WriteLine($"Nr of users from {instance} is {userIDs.Count()}");
                foreach (var userId in userIDs)
                {
                    file.WriteLine(userId);
                }
            }
            Console.WriteLine("User IDs saved to MastodonITALY.txt");
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

        private static async Task<IEnumerable<long>> GetUserDetailsAsync(MastodonClient client, HashSet<long> userIDs, string instance, string accountId)
        {
            var userId = long.Parse(accountId);

            var followers = await Retry(() => client.GetAccountFollowers(accountId), TimeSpan.FromSeconds(60), 7);
            var following = await Retry(() => client.GetAccountFollowing(accountId), TimeSpan.FromSeconds(60), 7);

            var newUsers = followers.Concat(following)
                .Where(user => !userIDs.Contains(long.Parse(user.Id)) && user.ProfileUrl.Contains(instance))
                .Select(user => long.Parse(user.Id));

            return newUsers;
        }
    }
}
