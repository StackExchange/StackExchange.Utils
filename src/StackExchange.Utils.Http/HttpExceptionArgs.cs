using System;

namespace StackExchange.Utils
{
    /// <summary>
    /// The event args for an exception.
    /// </summary>
    public class HttpExceptionArgs
    {
        /// <summary>
        /// The builder used to create the request.
        /// </summary>
        public IRequestBuilder Builder { get; }

        /// <summary>
        /// The error that was thrown.
        /// </summary>
        public Exception Error { get; }

        /// <summary>
        /// Whether to abort logging of this exception.
        /// </summary>
        public bool AbortLogging { get; set; }

        internal HttpExceptionArgs(IRequestBuilder builder, Exception ex)
        {
            Builder = builder;
            Error = ex;
        }
    }
}
