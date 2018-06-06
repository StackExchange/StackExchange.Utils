using System.Net.Http;

namespace StackExchange.Utils
{
    /// <summary>
    /// A pool implementation for <see cref="HttpClient"/> pooling.
    /// </summary>
    public interface IHttpClientPool
    {
        /// <summary>
        /// Gets a client for the specified <see cref="IRequestBuilder"/>.
        /// </summary>
        /// <param name="builder">The builder to get a client for.</param>
        /// <returns>A <see cref="HttpClient"/> from the pool.</returns>
        HttpClient Get(IRequestBuilder builder);

        /// <summary>
        /// Clears the pool, in case you need to.
        /// </summary>
        void Clear();
    }
}
