using System;
using System.Runtime.Serialization;

namespace StackExchange.Utils
{
    /// <summary>
    /// An exception from an <see cref="Http"/> call.
    /// </summary>
    public class HttpClientException : Exception
    {
        internal HttpClientException() { }
        internal HttpClientException(string message) : base(message) { }
        internal HttpClientException(string message, Exception innerException) : base(message, innerException) { }
        internal HttpClientException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
