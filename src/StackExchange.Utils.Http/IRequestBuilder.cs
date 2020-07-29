using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace StackExchange.Utils
{
    /// <summary>
    /// A request construct for building request options before issuing.
    /// </summary>
    public interface IRequestBuilder
    {
        /// <summary>
        /// The settings to use for this request. If null, <see cref="Http.DefaultSettings"/> should be used.
        /// </summary>
        HttpSettings Settings { get; }

        /// <summary>
        /// The request message being built.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        HttpRequestMessage Message { get; }

        /// <summary>
        /// Whether to log errors.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool LogErrors { get; set; }

        /// <summary>
        /// Which <see cref="HttpStatusCode"/>s to ignore as errors on responses.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerable<HttpStatusCode> IgnoredResponseStatuses { get; set; }

        /// <summary>
        /// The timeout to use on this request.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        TimeSpan Timeout { get; set; }

        /// <summary>
        /// The HttpCompletionOption to use on this request.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        HttpCompletionOption CompletionOption { get; set; }

        /// <summary>
        /// The Proxy to use when making requests
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        IWebProxy Proxy { get; set; }

        /// <summary>
        /// The <see cref="IHttpClientPool"/> to get a client from.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        IHttpClientPool ClientPool { get; set; }

        /// <summary>
        /// An before-logging event to call in case on an error.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        event EventHandler<HttpExceptionArgs> BeforeExceptionLog;

        /// <summary>
        /// Called before an exception is logged.
        /// </summary>
        /// <param name="args">The arguments wrapper for the exception thrown.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        void OnBeforeExceptionLog(HttpExceptionArgs args);

        /// <summary>
        /// Adds a handler to this request builder.
        /// </summary>
        /// <typeparam name="T">The payload type for this request.</typeparam>
        /// <param name="handler">The handler to add to this builder, used for extensions.</param>
        /// <returns>A wrapping <see cref="IRequestBuilder{T}"/> with the handler.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        IRequestBuilder<T> WithHandler<T>(Func<HttpResponseMessage, Task<T>> handler);
    }

    /// <summary>
    /// A typed request construct for building request options before issuing.
    /// </summary>
    /// <typeparam name="T">The type this request will return.</typeparam>
    public interface IRequestBuilder<T>
    {
        /// <summary>
        /// The wrapped <see cref="IRequestBuilder"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        IRequestBuilder Inner { get; }

        /// <summary>
        /// The response handler for this request.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        Func<HttpResponseMessage, Task<T>> Handler { get; }
    }
}
