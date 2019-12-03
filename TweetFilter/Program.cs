using LinqToTwitter;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using System;
using System.Threading.Tasks;

namespace TweetFilter
{
    //https://twitter.com/beerloverbuddy
    class Program
    {
        public static string TwitterConsumerKey = Environment.GetEnvironmentVariable("DEV_TWITTER_CONSUMER_KEY", EnvironmentVariableTarget.User);
        public static string TwitterConsumerSecret = Environment.GetEnvironmentVariable("DEV_TWITTER_CONSUMER_SECRET", EnvironmentVariableTarget.User);
        public static string TwitterAccessToken = Environment.GetEnvironmentVariable("DEV_TWITTER_ACCESS_TOKEN", EnvironmentVariableTarget.User);
        public static string TwitterAccessTokenSecret = Environment.GetEnvironmentVariable("DEV_TWITTER_ACCESS_TOKEN_SECRET", EnvironmentVariableTarget.User);
        public static string CognitiveServicesKey = Environment.GetEnvironmentVariable("DEV_COGNITIVE_SERVICES_KEY", EnvironmentVariableTarget.User);
        public static string CognitiveServicesEndpoint = Environment.GetEnvironmentVariable("DEV_COGNITIVE_SERVICES_ENDPOINT", EnvironmentVariableTarget.User);

        static async Task Main(string[] args)
        {
            Console.WriteLine("Get tweets!");

            var auth = new SingleUserAuthorizer
            {
                CredentialStore = new SingleUserInMemoryCredentialStore()
                {
                    ConsumerKey = TwitterConsumerKey,
                    ConsumerSecret = TwitterConsumerSecret,
                    AccessToken = TwitterAccessToken,
                    AccessTokenSecret = TwitterAccessTokenSecret
                }
            };
            await auth.AuthorizeAsync();

            var twitterCtx = new TwitterContext(auth);
            var tweets = await twitterCtx.GetTweets("cloudbrew");

            var credentials = new ApiKeyServiceClientCredentials(CognitiveServicesKey);
            var client = new TextAnalyticsClient(credentials)
            {
                Endpoint = CognitiveServicesEndpoint
            };

            foreach (var t in tweets)
            {
                var sentiment = await client.SentimentAsync(t.FullText, t.Lang);
                Console.WriteLine($"ID   : {t.StatusID}");
                Console.WriteLine($"User : {t.User.Name}");
                Console.WriteLine($"Text : {t.FullText}");
                Console.WriteLine($"Score: {sentiment.Score:0.00}");

                if(sentiment.Score > 0.5)
                {
                    await twitterCtx.Retweet(t.StatusID);
                    Console.WriteLine("Retweeted!");
                }
            }
            Console.WriteLine("End");
            Console.ReadLine();
        }
    }
}
