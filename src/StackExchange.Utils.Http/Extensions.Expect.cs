using Jil;
using ProtoBuf;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace StackExchange.Utils
{
    public static partial class ExtensionsForHttp
    {
        /// <summary>
        /// Sets the response handler for this request to a <see cref="bool"/> (200-299 response code).
        /// </summary>
        /// <param name="builder">The builder we're working on.</param>
        /// <returns>A typed request builder for chaining.</returns>
        public static IRequestBuilder<bool> ExpectHttpSuccess(this IRequestBuilder builder) =>
            builder.WithHandler(responseMessage => Task.FromResult(responseMessage.IsSuccessStatusCode));

        /// <summary>
        /// Holds handlers for ExpectJson(T) calls, so we don't re-create them in the common "default Options" case.
        ///
        /// Without this, we create a new Func for each ExpectJson call even
        /// </summary>
        /// <typeparam name="T">The type being deserialized.</typeparam>
        private static class JsonHandler<T>
        {
            internal static Func<HttpResponseMessage, Task<T>> WithOptions(IRequestBuilder builder, Options jsonOptions)
            {
                return async responseMessage =>
                {
                    using (var responseStream = await responseMessage.Content.ReadAsStreamAsync()) // Get the response here
                    using (var streamReader = new StreamReader(responseStream))                    // Stream reader
                    using (builder.GetSettings().ProfileGeneral?.Invoke("Deserialize: JSON"))
                    {
                        if (responseStream.Length == 0)
                        {
                            return default;
                        }

                        return JSON.Deserialize<T>(streamReader, jsonOptions ?? Options.Default);
                    }
                };
            }
        }

        /// <summary>
        /// Sets the response handler for this request to a JSON deserializer.
        /// </summary>
        /// <typeparam name="T">The type to Jil-deserialize to.</typeparam>
        /// <param name="builder">The builder we're working on.</param>
        /// <returns>A typed request builder for chaining.</returns>
        public static IRequestBuilder<T> ExpectJson<T>(this IRequestBuilder builder) => ExpectJson<T>(builder, Options.Default);

        /// <summary>
        /// Sets the response handler for this request to a JSON deserializer.
        /// </summary>
        /// <typeparam name="T">The type to Jil-deserialize to.</typeparam>
        /// <param name="builder">The builder we're working on.</param>
        /// <param name="jsonOptions">The Jil options to use when serializing.</param>
        /// <returns>A typed request builder for chaining.</returns>
        public static IRequestBuilder<T> ExpectJson<T>(this IRequestBuilder builder, Options jsonOptions) =>
            builder.WithHandler(JsonHandler<T>.WithOptions(builder, jsonOptions));

        /// <summary>
        /// Sets the response handler for this request to a protobuf deserializer.
        /// </summary>
        /// <typeparam name="T">The type to protobuf-deserialize to.</typeparam>
        /// <param name="builder">The builder we're working on.</param>
        /// <returns>A typed request builder for chaining.</returns>
        public static IRequestBuilder<T> ExpectProtobuf<T>(this IRequestBuilder builder) =>
            builder.WithHandler(async responseMessage =>
            {
                using (var responseStream = await responseMessage.Content.ReadAsStreamAsync())
                using (builder.GetSettings().ProfileGeneral?.Invoke("Deserialize: Protobuf"))
                {
                    return Serializer.Deserialize<T>(responseStream);
                }
            });

        /// <summary>
        /// Sets the response handler for this request to return the response as a <see cref="T:byte[]"/>.
        /// </summary>
        /// <param name="builder">The builder we're working on.</param>
        /// <returns>A typed request builder for chaining.</returns>
        public static IRequestBuilder<byte[]> ExpectByteArray(this IRequestBuilder builder) =>
            builder.WithHandler(responseMessage => responseMessage.Content.ReadAsByteArrayAsync());

        /// <summary>
        /// Sets the response handler for this request to return the response as a <see cref="string"/>.
        /// </summary>
        /// <param name="builder">The builder we're working on.</param>
        /// <returns>A typed request builder for chaining.</returns>
        public static IRequestBuilder<string> ExpectString(this IRequestBuilder builder) =>
            builder.WithHandler(async responseMessage =>
            {
                using (var responseStream = await responseMessage.Content.ReadAsStreamAsync())
                using (var streamReader = new StreamReader(responseStream))
                {
                    return await streamReader.ReadToEndAsync();
                }
            });
    }
}
