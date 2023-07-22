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

            var userIDs = new HashSet<long>(); 

            string max_id = "";
            string since_id = null;

            int offset = 0;
            const int limit = 80;
            int count = 0;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (userIDs.Count < 50423)
            {                
                var directoryAccounts = await client.GetDirectory(offset, limit, Mastonet.DirectoryOrder.Active, true);

                foreach (var account in directoryAccounts)
                {
                    try {
                        long userId = long.Parse(account.Id);
                        
                        //Check for duplicates (don't check for a user we've already checked their status before)
                        if ( !userIDs.Contains(userId))
                        {       
                            var followers = await Retry(() => client.GetAccountFollowers(userId.ToString()), TimeSpan.FromSeconds(60), 7);
                            var following = await Retry(() => client.GetAccountFollowing(userId.ToString()), TimeSpan.FromSeconds(60), 7);
                            
                            userIDs.Add(userId);

                            foreach (var user in followers.Concat(following))
                            {
                                if (!userIDs.Contains(long.Parse(user.Id)) && user.ProfileUrl.Contains(instance) && userIDs.Count < 50423) 
                                {
                                    var followers2 = await Retry(() => client.GetAccountFollowers(user.Id.ToString()), TimeSpan.FromSeconds(60), 7);
                                    var following2 = await Retry(() => client.GetAccountFollowing(user.Id.ToString()), TimeSpan.FromSeconds(60), 7);

                                    userIDs.Add(long.Parse(user.Id));

                                    foreach (var user2 in followers2.Concat(following2))
                                    {
                                        if (!userIDs.Contains(long.Parse(user2.Id)) && user2.ProfileUrl.Contains(instance) && userIDs.Count < 50423){  
                                            userIDs.Add(long.Parse(user2.Id));
                                        }
                                    }
                                    Console.WriteLine($"reached {userIDs.Count} users so far");
                                    count = 0;
                                }
                            }
                            Console.WriteLine($"reached {userIDs.Count} users     2ND for loop");
                            count = 0;
                        }
                    } catch (JsonReaderException ex){
                        Console.WriteLine($"Failed to parse JSON for account {account.Id}: {ex.Message}");
                        continue; // skip to the next account
                    }
                }
                Console.WriteLine($"reached {userIDs.Count} users Directory LOOP");
                count++;
                if (count >= 8){
                    Console.WriteLine("Reached the maximum count, pausing for 4 minutes...");
                    await Task.Delay(TimeSpan.FromMinutes(4));
                    count = 0; // reset the count after waiting
                }
                offset += limit;
            }

            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Console.WriteLine("RunTime to gather user IDS is " + elapsedTime);

            using (var file = new StreamWriter("MSocialID-50k_Active.txt"))
            {
                file.WriteLine($"Nr of users from {instance} is {userIDs.Count()}");
                foreach (var userId in userIDs)
                {
                    file.WriteLine(userId);
                }
            }
            Console.WriteLine("User IDs saved to MSocialID-50k_Active.txt");
        }

        //Retry function that delays task until cooldown of Mastodon API limit is finished
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