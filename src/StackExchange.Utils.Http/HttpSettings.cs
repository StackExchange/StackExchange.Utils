using System;
using System.Net;
using System.Net.Http;

namespace StackExchange.Utils
{
    /// <summary>
    /// Settings for <see cref="Http"/>.
    /// </summary>
    public class HttpSettings
    {
        /// <summary>
        /// The user agent to use on requests.
        /// </summary>
        public string UserAgent { get; set; } = "StackExchange.Utils HttpClient";

        /// <summary>
        /// The prefix to use on .Data[key] calls with additional debug data.
        /// Defaults to supporting StackExchange.Exceptional.
        /// </summary>
        public string ErrorDataPrefix { get; set; } = "ExceptionalCustom-";

        /// <summary>
        /// Allows modification of a request before it's sent.
        /// </summary>
        public event EventHandler<IRequestBuilder> BeforeSend;

        /// <summary>
        /// Handler for when an exception is thrown.
        /// </summary>
        public event EventHandler<HttpExceptionArgs> Exception;

        /// <summary>
        /// Profiling the request itself - from beginning to end.
        /// </summary>
        /// <example>
        /// Http.Settings.ProfileRequest = request => Current.ProfileHttp(request.Method.Method, request.RequestUri.ToString());
        /// </example>
        public Func<HttpRequestMessage, IDisposable> ProfileRequest { get; set; } //request => Current.ProfileHttp(request.Method.Method, request.RequestUri.ToString());

        /// <summary>
        /// Profiling deserialization steps like JSON or protobuf.
        /// </summary>
        /// <example>
        /// Http.Settings.ProfileGeneral = name => MiniProfiler.Current.Step(name);
        /// </example>
        public Func<string, IDisposable> ProfileGeneral { get; set; }

        /// <summary>
        /// The <see cref="IHttpClientPool"/> to use for <see cref="HttpClient"/> pooling. Defaults to <see cref="DefaultHttpClientPool"/>.
        /// </summary>
        public IHttpClientPool ClientPool { get; set; }

        /// <summary>
        /// The default timeout to use for requests.
        /// </summary>
        public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// The default Proxy to use when making requests.
        /// 
        /// This should create a new instance of a proxy when called,
        /// so that modifications don't affect the default (e.g.,
        /// changing Proxy.Credentials on a builder.Proxy should
        /// not affect the Proxy.Credentials used by default)
        /// </summary>
        public Func<IWebProxy> DefaultProxyFactory { get; set; } = null;

        internal void OnBeforeSend(object sender, IRequestBuilder builder) => BeforeSend?.Invoke(sender, builder);
        internal void OnException(object sender, HttpExceptionArgs args) => Exception?.Invoke(sender, args);

        /// <summary>
        /// Creates a new <see cref="HttpSettings"/>.
        /// </summary>
        public HttpSettings()
        {
            // Default pool by default
            ClientPool = new DefaultHttpClientPool(this);
        }
    }
}
