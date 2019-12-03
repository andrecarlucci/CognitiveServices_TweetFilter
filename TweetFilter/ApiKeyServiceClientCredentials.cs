using LinqToTwitter;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TweetFilter
{
    class ApiKeyServiceClientCredentials : ServiceClientCredentials
    {
        private readonly string apiKey;

        public ApiKeyServiceClientCredentials(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            request.Headers.Add("Ocp-Apim-Subscription-Key", this.apiKey);
            return base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }

    public static class TwitterContextExt
    {
        public static async Task<List<Status>> GetTweets(this TwitterContext context, string query)
        {
            var search = await context.Search.Where(s =>
                s.Type == SearchType.Search &&
                s.Query == query + " lang:en" &&
                s.IncludeEntities == true &&
                s.TweetMode == TweetMode.Extended
            ).SingleOrDefaultAsync();
            return search.Statuses;
        }

        public static async Task<bool> Retweet(this TwitterContext context, ulong statusId)
        {
            try
            {
                await context.RetweetAsync(statusId);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}