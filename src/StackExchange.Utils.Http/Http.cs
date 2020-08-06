using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace StackExchange.Utils
{
    /// <summary>
    /// Helper for making <see cref="HttpClient"/> calls.
    /// </summary>
    public static class Http
    {
        /// <summary>
        /// Current global settings for <see cref="Http"/>.
        /// </summary>
        public static HttpSettings DefaultSettings { get; } = new HttpSettings();

        /// <summary>
        /// Gets a new request at the specified URL.
        /// </summary>
        /// <param name="uri">The URI we're making a request to (this client takes care of .internal itself).</param>
        /// <param name="settings">(Optional) The specific <see cref="HttpSettings"/> to use for this request.</param>
        /// <param name="callerName">The caller member name, auto-populated by the compiler for debugging info.</param>
        /// <param name="callerFile">The caller file path, auto-populated by the compiler for debugging info.</param>
        /// <param name="callerLine">The caller file line number, auto-populated by the compiler for debugging info.</param>
        /// <returns>A chaining builder for your request.</returns>
        public static IRequestBuilder Request(
            string uri,
            HttpSettings settings = null,
            [CallerMemberName] string callerName = null,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0) => new HttpBuilder(uri, settings, callerName, callerFile, callerLine);

        private static readonly FieldInfo stackTraceString = typeof(Exception).GetField("_stackTraceString", BindingFlags.Instance | BindingFlags.NonPublic);

        internal static async Task<HttpCallResponse<T>> SendAsync<T>(IRequestBuilder<T> builder, HttpMethod method, CancellationToken cancellationToken = default)
        {
            // default to global settings
            var settings = builder.GetSettings();

            settings.OnBeforeSend(builder, builder.Inner);

            var request = builder.Inner.Message;
            request.Method = method;

            Exception exception = null;
            HttpResponseMessage response = null;
            try
            {
                using (settings.ProfileRequest?.Invoke(request))
                using (request)
                {
                    // Get the pool
                    var pool = builder.Inner.ClientPool ?? settings.ClientPool;

                    var completionOption = builder.Inner.BufferResponse ? HttpCompletionOption.ResponseContentRead : HttpCompletionOption.ResponseHeadersRead;
                    
                    // Send the request
                    using (response = await pool.Get(builder.Inner).SendAsync(request, completionOption, cancellationToken))
                    {
                        // If we haven't ignored it, throw and we'll log below
                        // This isn't ideal cntrol flow behavior, but it's the only way to get proper stacks
                        if (!response.IsSuccessStatusCode && !builder.Inner.IgnoredResponseStatuses.Contains(response.StatusCode))
                        {
                            exception = new HttpClientException($"Response code was {(int)response.StatusCode} ({response.StatusCode}) from {response.RequestMessage.RequestUri}: {response.ReasonPhrase}", response.StatusCode, response.RequestMessage.RequestUri);
                            stackTraceString.SetValue(exception, new StackTrace(true).ToString());
                        }
                        else
                        {
                            var data = await builder.Handler(response);
                            return HttpCallResponse.Create(response, data);
                        }
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                exception = cancellationToken.IsCancellationRequested
                    ? new HttpClientException("HttpClient request cancelled by token request.", builder.Inner.Message.RequestUri, ex)
                    : new HttpClientException("HttpClient request timed out. Timeout: " + builder.Inner.Timeout.TotalMilliseconds.ToString("N0") + "ms", builder.Inner.Message.RequestUri, ex);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Use the response if we have it - request if not
            var result = response != null
                ? HttpCallResponse.Create<T>(response, exception)
                : HttpCallResponse.Create<T>(request, exception);

            // Add caller member bits to the exception data regardless of where it's eventually logged.
            (builder.Inner as HttpBuilder)?.AddExceptionData(exception);

            // If we're told not to log at all, don't log
            if (builder.Inner.LogErrors)
            {
                var args = new HttpExceptionArgs(builder.Inner, exception);
                builder.Inner.OnBeforeExceptionLog(args);

                if (!args.AbortLogging)
                {
                    settings.OnException(builder, args);
                }
            }

            return result;
        }
    }
}
