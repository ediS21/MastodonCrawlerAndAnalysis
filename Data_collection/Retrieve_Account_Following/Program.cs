 using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Mastonet;
using Mastonet.Entities;
using Newtonsoft.Json;
using System.Text.Json;

namespace MastodonID
{
    class Program
    {
        static async Task Main(string[] args)
        {   //Input file with user IDs and instance from which the data is retrieved
            string filePath = "MastodonNL-data.txt";
            string instance = "mastodon.nl";

            //Access to Mastodon API via Mastonet
            var clientKey = "Ln0AUYiIIrEM4gt0ZSZhuCfPYWnJKu6CPLJaSssTOrw";
            var clientSecret = "hZLt9rtGYPWvqOMuk92g0Yy2jopD8iFC_rYjUSkB6Dc";
            var accessToken = "trt-7sOKiyvkz_rRC8huQOo4e5GNrfG7jB9dLzyTVc4";

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

            //List of user ids
            List<long> userIDs = new List<long>();

            //Used a stopwatch to time how long the process takes
            //Showed time every time the program stops to wait down the rate limit of Mastodon's API
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            //Add userIDs from input file to List
            using (StreamReader reader = new StreamReader(filePath)){
                string line;
                
                while ((line = reader.ReadLine()) != null)
                {
                    // Parse the JSON line and extract the "id" field
                    var idTokenIndex = line.IndexOf("\"id\":\"");
                    if (idTokenIndex != -1)
                    {
                        idTokenIndex += 6; // Move to the end of the "id":" part
                        var endIndex = line.IndexOf("\"", idTokenIndex);
                        if (endIndex != -1)
                        {
                            var idString = line.Substring(idTokenIndex, endIndex - idTokenIndex);
                            if (long.TryParse(idString, out long userID))
                            {
                                userIDs.Add(userID);
                            }
                            else
                            {
                                Console.WriteLine($"Failed to parse user ID: {idString}");
                            }
                        }
                    }
                }
            }

            int counter = 0;
            int followerCounter = 0;

            //Main function to retrieve data and write it in an output file
            using (var file = new StreamWriter("FollowingNL.json"))
            {
                file.WriteLine($"Nr of users from {instance} is {userIDs.Count()}");
                foreach (var userId in userIDs)
                {
                    // Retrieve accounts the user with id "userID" is following
                    var account = await Retry(() => client.GetAccountFollowing(userId.ToString()), TimeSpan.FromSeconds(60), 7);
                    var filteredAccounts = account.Select(a => new { source_id = userId, target_id = a.Id, a.ProfileUrl });

                    if (!filteredAccounts.Any())
                    {
                        var noFollowEntry = new[] { new { source_id = userId, target_id = (long?)null, ProfileUrl = (string)null } };
                        file.WriteLine(System.Text.Json.JsonSerializer.Serialize(noFollowEntry));
                    }
                    else
                    {
                        var serializedAccount = System.Text.Json.JsonSerializer.Serialize(filteredAccounts);
                        file.WriteLine(serializedAccount);
                    }

                    var maxId = account.NextPageMaxId;
                    while(maxId != null) // If maxId is null, there are no more followers to fetch
                    {
                        var moreFollowers = await Retry(() => client.GetAccountFollowing(userId.ToString(), new ArrayOptions() { MaxId = maxId }), TimeSpan.FromSeconds(60), 7);
                        filteredAccounts = moreFollowers.Select(a => new { source_id = userId, target_id = a.Id, a.ProfileUrl });
                        
                        if (!filteredAccounts.Any())  // check if the following list is empty
                        {
                            var noFollowEntry = new[] { new { source_id = userId, target_id = (long?)null, ProfileUrl = (string)null } };
                            file.WriteLine(JsonConvert.SerializeObject(noFollowEntry));
                            break; // break the loop as there are no more followers to fetch
                        }
                        else
                        {
                            var serializedAccount = JsonConvert.SerializeObject(filteredAccounts);
                            file.WriteLine(serializedAccount);
                        }

                        counter++;
                        Console.WriteLine(counter);

                        maxId = moreFollowers.NextPageMaxId; // Update maxId for the next iteration
                    }
                    /*
                    Keep track of the number of mastodon.social accounts the program has reached so far
                    as the program would usually for multiple hours (from morning till evening with a couple of breaks in between)
                    */
                    followerCounter++;
                    counter = 0;
                    Console.WriteLine($"################ Reached {followerCounter} users");
                }
            }
            Console.WriteLine("User IDs saved to FollowingNL.json");

            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Console.WriteLine("RunTime to gather user IDS is " + elapsedTime);
        }

        //Function to Retry Mastodon Endpoints after 300 requests
        static async Task<T> Retry<T>(Func<Task<T>> func, TimeSpan delay, int maxRetries)
        {
            int retries = 0;
            while (true)
            {
                try
                {
                    return await func();
                }

                //Main ServiceErrorException that delays accessing Mastodon Endpoint until the cooldown time is finished
                catch (ServerErrorException ex)
                {
                    if (ex.Message.Contains("Too many requests"))
                    {
                        Console.WriteLine("Too many requests, waiting for 60 seconds before retrying");
                        await Task.Delay(TimeSpan.FromSeconds(60));
                        continue;
                    }

                    //Retrieved empty/no data -> skip
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

                //Network Error. If program loses internet connection, this exception will delay accessing Mastodon endpoints
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

                //JSON exception 
                catch(JsonReaderException ex){
                    if (ex.Message.Contains("Could not convert string to DateTime"))
                    {
                        Console.WriteLine("Could not convert string to DateTime, skipping user");
                        return default(T);
                    } 
                    else {
                        Console.WriteLine($"Json conversion error: {ex.Message}");
                        throw;
                    }
                }
            }
        }
    }
}