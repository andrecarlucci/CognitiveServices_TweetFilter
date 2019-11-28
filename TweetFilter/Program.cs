using LinqToTwitter;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TweetFilter
{
    class Program
    {
        public static string TwitterConsumerKey = Environment.GetEnvironmentVariable("DEV_TWITTER_CONSUMER_KEY", EnvironmentVariableTarget.User);
        public static string TwitterConsumerSecret = Environment.GetEnvironmentVariable("DEV_TWITTER_CONSUMER_SECRET", EnvironmentVariableTarget.User);
        public static string CognitiveServicesKey = Environment.GetEnvironmentVariable("DEV_COGNITIVE_SERVICES_KEY", EnvironmentVariableTarget.User);
        public static string CognitiveServicesEndpoint = Environment.GetEnvironmentVariable("DEV_COGNITIVE_SERVICES_ENDPOINT", EnvironmentVariableTarget.User);


        static async Task Main(string[] args)
        {
            Console.WriteLine("Get tweets!");

            var auth = new ApplicationOnlyAuthorizer
            {
                CredentialStore = new InMemoryCredentialStore()
                {
                    ConsumerKey = TwitterConsumerKey,
                    ConsumerSecret = TwitterConsumerSecret
                }
            };
            await auth.AuthorizeAsync();

            var twitterCtx = new TwitterContext(auth);
            var search = await GetTweets(twitterCtx);

            var credentials = new ApiKeyServiceClientCredentials(CognitiveServicesKey);
            var client = new TextAnalyticsClient(credentials)
            {
                Endpoint = CognitiveServicesEndpoint
            };

            Console.WriteLine("\nQuery: {0}\n", search.SearchMetaData.Query);
            foreach (var t in search.Statuses)
            {
                var sentiment = await client.SentimentAsync(t.FullText, t.Lang);
                Console.WriteLine($"ID   : {t.ID:(0,-15)}");
                Console.WriteLine($"User : {t.User.Name}");
                Console.WriteLine($"Text : {t.FullText}");
                Console.WriteLine($"Score: {sentiment.Score:0.00}");
            }
        }

        private static async Task<Search> GetTweets(TwitterContext twitterCtx)
        {
            var search = await twitterCtx.Search.Where(s =>
                s.Type == SearchType.Search &&
                s.Query == "westvleteren lang:en" &&
                s.IncludeEntities == true &&
                s.TweetMode == TweetMode.Extended
            ).SingleOrDefaultAsync();
            return search;
        }
    }
}
