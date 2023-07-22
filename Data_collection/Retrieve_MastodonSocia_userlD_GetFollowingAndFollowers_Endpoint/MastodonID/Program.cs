using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Mastonet;
using Mastonet.Entities;

namespace MastodonID
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string instance = "mastodon.social";
            string targetInstance = "mastodon.cloud";

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

            // store tuple of (userId, followersFromTargetInstance, followingFromTargetInstance)
            var userIDs = new HashSet<(long, int, int)>(); 

            string max_id = "";
            string since_id = null;
            int targetFollowing = 0;
            int sourceFollowers = 0;
            int averageFollowersFromTarget = 0;
            int averageFollowingFromSource = 0;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (userIDs.Count < 11423)
            {
                var timeline_params = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(max_id))
                {
                    timeline_params["max_id"] = max_id;
                }
                if (since_id != null)
                {
                    timeline_params["since_id"] = since_id;
                }

                //Local timeline only
                var localTimeline = await client.GetPublicTimeline(local: true);

                if (localTimeline.Count == 0)
                {
                    Console.WriteLine("No more statuses found. Exiting loop.");
                    break;
                }

                // Update the max_id and since_id values
                max_id = localTimeline.Min(s => s.Id);
                since_id = localTimeline.Max(s => s.Id);

                foreach (var status in localTimeline)
                {
                    long userId = long.Parse(status.Account.Id);
                    
                    //Check for duplicates (don't check for a user we've already checked their status before)
                    if ( !userIDs.Any(x => x.Item1 == userId))
                    {
                            
                        var followers = await Retry(() => client.GetAccountFollowers(userId.ToString()), TimeSpan.FromSeconds(60), 7);
                        var following = await Retry(() => client.GetAccountFollowing(userId.ToString()), TimeSpan.FromSeconds(60), 7);
                        
                        int followersFromTargetInstance = followers.Count(f => f.ProfileUrl.Contains(targetInstance));
                        averageFollowersFromTarget += followersFromTargetInstance;
                        int followingFromTargetInstance = following.Count(f => f.ProfileUrl.Contains(targetInstance));
                        averageFollowingFromSource += followingFromTargetInstance;
                        
                        if (followersFromTargetInstance > 0){
                            sourceFollowers++;
                        }
                        if (followingFromTargetInstance > 0){
                            targetFollowing++;
                        }

                        userIDs.Add((userId,followersFromTargetInstance,followingFromTargetInstance));

                        foreach (var user in followers.Concat(following))
                        {
                            if (!userIDs.Any(x => x.Item1 == long.Parse(user.Id)) && user.ProfileUrl.Contains(instance) && userIDs.Count < 11423) 
                            {
                                var followers2 = await Retry(() => client.GetAccountFollowers(user.Id.ToString()), TimeSpan.FromSeconds(60), 7);
                                var following2 = await Retry(() => client.GetAccountFollowing(user.Id.ToString()), TimeSpan.FromSeconds(60), 7);

                                int followersFromTargetInstance2 = followers.Count(f => f.ProfileUrl.Contains(targetInstance));
                                averageFollowersFromTarget += followersFromTargetInstance2;
                                int followingFromTargetInstance2 = following.Count(f => f.ProfileUrl.Contains(targetInstance));
                                averageFollowingFromSource += followingFromTargetInstance2;

                                if (followersFromTargetInstance2 > 0){
                                    sourceFollowers++;
                                }
                                if (followingFromTargetInstance2 > 0){
                                    targetFollowing++;
                                }

                                userIDs.Add((long.Parse(user.Id), followersFromTargetInstance2, followingFromTargetInstance2));

                                foreach (var user2 in followers2.Concat(following2))
                                {
                                    if (!userIDs.Any(x => x.Item1 == long.Parse(user2.Id)) && user2.ProfileUrl.Contains(instance) && userIDs.Count < 11423){
                                        var followers3 = await Retry(() => client.GetAccountFollowers(user2.Id.ToString()), TimeSpan.FromSeconds(60), 7);
                                        var following3 = await Retry(() => client.GetAccountFollowing(user2.Id.ToString()), TimeSpan.FromSeconds(60), 7);

                                        int followersFromTargetInstance3 = followers.Count(f => f.ProfileUrl.Contains(targetInstance));
                                        averageFollowersFromTarget += followersFromTargetInstance2;
                                        int followingFromTargetInstance3 = following.Count(f => f.ProfileUrl.Contains(targetInstance));
                                        averageFollowingFromSource += followingFromTargetInstance2;  //TODO: correct this into following..3

                                        if (followersFromTargetInstance3 > 0){
                                            sourceFollowers++;
                                        }
                                        if (followingFromTargetInstance3 > 0){
                                            targetFollowing++;
                                        }
                                        
                                        userIDs.Add((long.Parse(user2.Id), followersFromTargetInstance3, followingFromTargetInstance3));
                                    }
                                }
                                Console.WriteLine($"reached {userIDs.Count} users so far");
                            }
                        }
                        Console.WriteLine($"reached {userIDs.Count} users     2ND for loop");
                    }
                }
                Console.WriteLine($"reached {userIDs.Count} users STATUSSTATUSSTATUS LOOP");
            }

            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Console.WriteLine("RunTime to gather user IDS is " + elapsedTime);

            using (var file = new StreamWriter("MSocialID-11kFF.txt"))
            {
                file.WriteLine($"Nr of users from {instance} is {userIDs.Count()}");
                foreach (var (userId, followersFromTargetInstance, followingFromTargetInstance) in userIDs)
                {
                    string output = $"User {userId} ({instance}) follows {followingFromTargetInstance} users and is followed by {followersFromTargetInstance} users from {targetInstance}";
                    file.WriteLine(output);
                }
                file.WriteLine($"Nr of users from {instance} who follow users from {targetInstance} is {targetFollowing}");
                file.WriteLine($"Nr of users from {instance} who are followed by users from {targetInstance} is {sourceFollowers}");
                averageFollowersFromTarget = averageFollowersFromTarget/userIDs.Count();
                averageFollowingFromSource = averageFollowingFromSource/userIDs.Count();
                file.WriteLine($"Average nr of users from {instance} who follow users from {targetInstance} is {averageFollowersFromTarget}");
                file.WriteLine($"Average nr of users from {instance} who are followed by users from {targetInstance} is {sourceFollowers}");
            }
            Console.WriteLine("User IDs saved to MSocialID-11kFF.txt");
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