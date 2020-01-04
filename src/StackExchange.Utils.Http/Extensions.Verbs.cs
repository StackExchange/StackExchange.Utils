using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace StackExchange.Utils
{
    public static partial class ExtensionsForHttp
    {
        /// <summary>
        /// Issue the request as a DELETE.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="builder">The builder used for this request.</param>
        /// <param name="cancellationToken">The cancellation token for stopping the request.</param>
        /// <returns>A <see cref="HttpCallResponse{T}"/> to consume.</returns>
        public static Task<HttpCallResponse<T>> DeleteAsync<T>(this IRequestBuilder<T> builder, CancellationToken cancellationToken = default) =>
            Http.SendAsync(builder, HttpMethod.Delete, cancellationToken);

        /// <summary>
        /// Issue the request as a GET.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="builder">The builder used for this request.</param>
        /// <param name="cancellationToken">The cancellation token for stopping the request.</param>
        /// <returns>A <see cref="HttpCallResponse{T}"/> to consume.</returns>
        public static Task<HttpCallResponse<T>> GetAsync<T>(this IRequestBuilder<T> builder, CancellationToken cancellationToken = default) =>
            Http.SendAsync(builder, HttpMethod.Get, cancellationToken);

        /// <summary>
        /// Issue the request as a POST.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="builder">The builder used for this request.</param>
        /// <param name="cancellationToken">The cancellation token for stopping the request.</param>
        /// <returns>A <see cref="HttpCallResponse{T}"/> to consume.</returns>
        public static Task<HttpCallResponse<T>> PostAsync<T>(this IRequestBuilder<T> builder, CancellationToken cancellationToken = default) =>
            Http.SendAsync(builder, HttpMethod.Post, cancellationToken);

        /// <summary>
        /// Issue the request as a PUT.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="builder">The builder used for this request.</param>
        /// <param name="cancellationToken">The cancellation token for stopping the request.</param>
        /// <returns>A <see cref="HttpCallResponse{T}"/> to consume.</returns>
        public static Task<HttpCallResponse<T>> PutAsync<T>(this IRequestBuilder<T> builder, CancellationToken cancellationToken = default) =>
            Http.SendAsync(builder, HttpMethod.Put, cancellationToken);

        /// <summary>
        /// Issue the request as a PATCH.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="builder">The builder used for this request.</param>
        /// <param name="cancellationToken">The cancellation token for stopping the request.</param>
        /// <returns>A <see cref="HttpCallResponse{T}"/> to consume.</returns>
        public static Task<HttpCallResponse<T>> PatchAsync<T>(this IRequestBuilder<T> builder, CancellationToken cancellationToken = default) =>
            Http.SendAsync(builder, new HttpMethod("PATCH"), cancellationToken);
    }
}
