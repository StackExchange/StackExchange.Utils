using Jil;
using ProtoBuf;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace StackExchange.Utils
{
    public static partial class ExtensionsForHttp
    {
        /// <summary>
        /// Sets the given <see cref="HttpContent"/> as the body for this request.
        /// </summary>
        /// <param name="builder">The builder we're working on.</param>
        /// <param name="content">The <see cref="HttpContent"/> to use.</param>
        /// <returns>The request builder for chaining.</returns>
        public static IRequestBuilder SendContent(this IRequestBuilder builder, HttpContent content)
        {
            builder.Message.Content = content;
            return builder;
        }

        /// <summary>
        /// Adds a <see cref="NameValueCollection"/> as the body for this request.
        /// </summary>
        /// <param name="builder">The builder we're working on.</param>
        /// <param name="form">The <see cref="NameValueCollection"/> (e.g. FormCollection) to use.</param>
        /// <returns>The request builder for chaining.</returns>
        public static IRequestBuilder SendForm(this IRequestBuilder builder, NameValueCollection form) =>
            SendContent(builder, new FormUrlEncodedContent(form.AllKeys.ToDictionary(k => k, v => form[v])));

        /// <summary>
        /// Adds raw HTML content as the body for this request.
        /// </summary>
        /// <param name="builder">The builder we're working on.</param>
        /// <param name="html">The raw HTML string to use.</param>
        /// <returns>The request builder for chaining.</returns>
        public static IRequestBuilder SendHtml(this IRequestBuilder builder, string html) =>
            SendContent(builder, new StringContent(html, Encoding.UTF8, "text/html"));


        /// <summary>
        /// Adds JSON (Jil-serialized) content as the body for this request.
        /// </summary>
        /// <param name="builder">The builder we're working on.</param>
        /// <param name="obj">The object to serialize as JSON in the body.</param>
        /// <returns>The request builder for chaining.</returns>
        public static IRequestBuilder SendJson(this IRequestBuilder builder, object obj) => SendJson(builder, obj, Options.Default);

        /// <summary>
        /// Adds JSON (Jil-serialized) content as the body for this request.
        /// </summary>
        /// <param name="builder">The builder we're working on.</param>
        /// <param name="obj">The object to serialize as JSON in the body.</param>
        /// <param name="jsonOptions">The Jil options to use when serializing.</param>
        /// <returns>The request builder for chaining.</returns>
        public static IRequestBuilder SendJson(this IRequestBuilder builder, object obj, Options jsonOptions) =>
            SendContent(builder, new StringContent(JSON.Serialize(obj, jsonOptions), Encoding.UTF8, "application/json"));

        /// <summary>
        /// Adds raw text content as the body for this request.
        /// </summary>
        /// <param name="builder">The builder we're working on.</param>
        /// <param name="text">The raw text string to use.</param>
        /// <returns>The request builder for chaining.</returns>
        public static IRequestBuilder SendPlaintext(this IRequestBuilder builder, string text) =>
            SendContent(builder, new StringContent(text, Encoding.UTF8, "application/x-www-form-urlencoded"));

        /// <summary>
        /// Adds protobuf-serialized content as the body for this request.
        /// </summary>
        /// <param name="builder">The builder we're working on.</param>
        /// <param name="obj">The object to serialize with protobuf in the body.</param>
        /// <returns>The request builder for chaining.</returns>
        public static IRequestBuilder SendProtobuf(this IRequestBuilder builder, object obj)
        {
            using (var output = new MemoryStream())
            using (var gzs = new GZipStream(output, CompressionMode.Compress))
            {
                Serializer.Serialize(gzs, obj);
                gzs.Close();
                var protoContent = new ByteArrayContent(output.ToArray());
                protoContent.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                protoContent.Headers.Add("Content-Encoding", "gzip");
                return SendContent(builder, protoContent);
            }
        }
    }
}
