using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace StackExchange.Utils
{
    /// <summary>
    /// The default implementation of <see cref="IHttpClientPool"/>.
    /// </summary>
    public class DefaultHttpClientPool : IHttpClientPool
    {
        private readonly ConcurrentDictionary<HttpClientCacheKey, HttpClient> ClientPool = new ConcurrentDictionary<HttpClientCacheKey, HttpClient>();
        private HttpSettings Settings { get; }

        /// <summary>
        /// Creates a new <see cref="DefaultHttpClientPool"/> based on the settings.
        /// </summary>
        /// <param name="settings">The settings to based this pool on.</param>
        public DefaultHttpClientPool(HttpSettings settings) => Settings = settings;

        /// <summary>
        /// Gets a <see cref="HttpClient"/> from the pool, based on the <see cref="IRequestBuilder"/>.
        /// </summary>
        /// <param name="builder">The builder to get a request from.</param>
        /// <returns>The found or created <see cref="HttpClient"/> from the pool.</returns>
        public HttpClient Get(IRequestBuilder builder) => ClientPool.GetOrAdd(new HttpClientCacheKey(builder.Timeout), CreateHttpClient);

        private HttpClient CreateHttpClient(HttpClientCacheKey options)
        {
            var handler = new HttpClientHandler
            {
                UseCookies = false
            };
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            }

            var client = new HttpClient(handler)
            {
                Timeout = options.Timeout,
                DefaultRequestHeaders =
                {
                    AcceptEncoding =
                    {
                        new StringWithQualityHeaderValue("gzip"),
                        new StringWithQualityHeaderValue("deflate")
                    }
                }
            };
            client.DefaultRequestHeaders.Add("User-Agent", Settings.UserAgent);
            return client;
        }

        /// <summary>
        /// Clears the pool, causing all new <see cref="HttpClient"/>s to be created on the following calls.
        /// </summary>
        public void Clear() => ClientPool.Clear();

        private struct HttpClientCacheKey
        {
            public TimeSpan Timeout { get; }

            public HttpClientCacheKey(TimeSpan timeout) => Timeout = timeout;
        }
    }
}
