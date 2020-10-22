using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace StackExchange.Utils
{
    public static partial class ExtensionsForHttp
    {
        /// <summary>
        /// Sets a proxy for this request.
        /// </summary>
        /// <param name="builder">The builder we're working on.</param>
        /// <param name="proxy">The proxy to use on this request.</param>
        /// <returns>The request builder for chaining.</returns>
        /// <remarks>
        /// This isn't *really* per request since it's global on <see cref="System.Net.Http.HttpClient"/>,
        /// so in reality we grab a different client from the pool.
        /// </remarks>
        public static IRequestBuilder WithProxy(this IRequestBuilder builder, IWebProxy proxy)
        {
            builder.Proxy = proxy;
            return builder;
        }

        /// <summary>
        /// Sets an <see cref="IHttpClientPool"/> to get a client from for this request.
        /// </summary>
        /// <param name="builder">The builder we're working on.</param>
        /// <param name="pool">The pool to use on this request (defaults to global settings otherwise).</param>
        /// <returns>The request builder for chaining.</returns>
        public static IRequestBuilder WithClientPool(this IRequestBuilder builder, IHttpClientPool pool)
        {
            builder.ClientPool = pool;
            return builder;
        }

        /// <summary>
        /// Sets a timeout for this request.
        /// </summary>
        /// <param name="builder">The builder we're working on.</param>
        /// <param name="timeout">The timeout to use on this request.</param>
        /// <returns>The request builder for chaining.</returns>
        /// <remarks>
        /// This isn't *really* per request since it's global on <see cref="System.Net.Http.HttpClient"/>,
        /// so in reality we grab a different client from the pool.
        /// </remarks>
        public static IRequestBuilder WithTimeout(this IRequestBuilder builder, TimeSpan timeout)
        {
            builder.Timeout = timeout;
            return builder;
        }

        /// <summary>
        /// Disables logging errors to the exceptional log on this request.
        /// </summary>
        /// <param name="builder">The builder we're working on.</param>
        /// <returns>The request builder for chaining.</returns>
        public static IRequestBuilder WithoutErrorLogging(this IRequestBuilder builder)
        {
            builder.LogErrors = false;
            return builder;
        }

        /// <summary>
        /// Doesn't log an error when the response's HTTP status code is any of the <paramref name="ignoredStatusCodes"/>.
        /// </summary>
        /// <param name="builder">The builder we're working on.</param>
        /// <param name="ignoredStatusCodes">HTTP status codes to ignore.</param>
        /// <returns>The request builder for chaining.</returns>
        public static IRequestBuilder WithoutLogging(this IRequestBuilder builder, IEnumerable<HttpStatusCode> ignoredStatusCodes)
        {
            builder.IgnoredResponseStatuses = ignoredStatusCodes;
            return builder;
        }

        private static readonly ConcurrentDictionary<HttpStatusCode, ImmutableHashSet<HttpStatusCode>> _ignoreCache = new ConcurrentDictionary<HttpStatusCode, ImmutableHashSet<HttpStatusCode>>();

        /// <summary>
        /// Doesn't log an error when the response's HTTP status code is <paramref name="ignoredStatusCode"/>.
        /// </summary>
        /// <param name="builder">The builder we're working on.</param>
        /// <param name="ignoredStatusCode">HTTP status code to ignore.</param>
        /// <returns>The request builder for chaining.</returns>
        public static IRequestBuilder WithoutLogging(this IRequestBuilder builder, HttpStatusCode ignoredStatusCode)
        {
            builder.IgnoredResponseStatuses = _ignoreCache.GetOrAdd(ignoredStatusCode, k => ImmutableHashSet.Create(k));
            return builder;
        }

        /// <summary>
        /// Adds an event handler for this request, for appending additional information to the logged exception for example.
        /// </summary>
        /// <param name="builder">The builder we're working on.</param>
        /// <param name="beforeLogHandler">The exception handler to run before logging</param>
        /// <returns>The request builder for chaining.</returns>
        public static IRequestBuilder OnException(this IRequestBuilder builder, EventHandler<HttpExceptionArgs> beforeLogHandler)
        {
            builder.BeforeExceptionLog += beforeLogHandler;
            return builder;
        }

        /// <summary>
        /// Add a header to this request.
        /// </summary>
        /// <param name="builder">The builder we're working on.</param>
        /// <param name="name">The header name to add to this request.</param>
        /// <param name="value">The header value (for <paramref name="name"/>) to add to this request.</param>
        /// <returns>The request builder for chaining.</returns>
        public static IRequestBuilder AddHeader(this IRequestBuilder builder, string name, string value)
        {
            if (!string.IsNullOrEmpty(name))
            {
                try
                {
                    builder.Message.Headers.Add(name, value);
                }
                catch (Exception e)
                {
                    var wrapper = new HttpClientException("Unable to set header: " + name + " to '" + value + "'", builder.Message.RequestUri, e);
                    builder.GetSettings().OnException(builder, new HttpExceptionArgs(builder, wrapper));
                }
            }
            return builder;
        }

        /// <summary>
        /// Adds a single header without Validation against known Header types.
        /// (ideal if you have different interpretation to the spec for any known types)
        /// </summary>
        /// <param name="builder">The builder we're working on.</param>
        /// <param name="name">The auth scheme to add to this request.</param>
        /// <param name="value">The key value (for <paramref name="name"/>) to add to this request.</param>
        /// <returns>The request builder for chaining.</returns>
        public static IRequestBuilder AddHeaderWithoutValidation(this IRequestBuilder builder, string name, string value)
        {
            if (!string.IsNullOrEmpty(name))
            {
                try
                {
                    builder.Message.Headers.TryAddWithoutValidation(name, value);
                }
                catch (Exception e)
                {
                    var wrapper = new HttpClientException("Unable to set header using: " + name + " to '" + value + "'", builder.Message.RequestUri, e);
                    builder.GetSettings().OnException(builder, new HttpExceptionArgs(builder, wrapper));
                }
            }
            return builder;
        }

        /// <summary>
        /// Adds headers to this request.
        /// </summary>
        /// <param name="builder">The builder we're working on.</param>
        /// <param name="headers">The headers to add to this request.</param>
        /// <returns>The request builder for chaining.</returns>
        public static IRequestBuilder AddHeaders(this IRequestBuilder builder, IDictionary<string, string> headers)
        {
            if (headers == null) return builder;

            var pHeaders = builder.Message.Headers;
            foreach (var kv in headers)
            {
                try
                {
                    //pHeaders.Add(kv.Key, kv.Value);
                    switch (kv.Key)
                    {
                        //    certain headers must be accessed via the named property on the WebRequest
                        case "Accept": pHeaders.Accept.ParseAdd(kv.Value); break;
                        //    case "Connection": break;
                        //    case "proxy-connection": break;
                        //    case "Proxy-Connection": break;
                        //    case "Content-Length": break;
                        case "Content-Type": builder.Message.Content.Headers.ContentType = new MediaTypeHeaderValue(kv.Value); break;
                        //    case "Host": break;
                        //    case "If-Modified-Since": pHeaders.IfModifiedSince = DateTime.ParseExact(kv.Value, "R", CultureInfo.InvariantCulture); break;
                        //    case "Referer": pHeaders.Referrer = new Uri(kv.Value); break;
                        //    case "User-Agent": pHeaders.UserAgent.ParseAdd("Stack Exchange (Proxy)"); break;
                        default: pHeaders.Add(kv.Key, kv.Value); break;
                    }
                }
                catch (Exception e)
                {
                    var wrapper = new HttpClientException("Unable to set header: " + kv.Key + " to '" + kv.Value + "'", builder.Message.RequestUri, e);
                    builder.GetSettings().OnException(builder, new HttpExceptionArgs(builder, wrapper));
                }
            }
            return builder;
        }

        /// <summary>
        /// Specifies the HTTP version to use for this request
        /// </summary>
        public static IRequestBuilder WithProtocolVersion(this IRequestBuilder builder, Version version)
        {
            builder.Message.Version = version;
            return builder;
        }

        /// <summary>
        /// Indicates that the response's content shouldn't be buffered, setting the HttpCompletionOption accordingly.
        /// </summary>
        public static IRequestBuilder WithoutResponseBuffering(this IRequestBuilder builder)
        {
            builder.BufferResponse = false;
            return builder;
        }
    }
}
