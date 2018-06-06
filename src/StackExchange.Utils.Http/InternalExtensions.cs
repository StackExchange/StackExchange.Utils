using System;

namespace StackExchange.Utils
{
    internal static class InternalExtensions
    {
        /// <summary>
        /// Gets the settings or default settings for a <see cref="IRequestBuilder{T}"/>.
        /// </summary>
        /// <param name="builder">The builder to get settings for.</param>
        public static HttpSettings GetSettings<T>(this IRequestBuilder<T> builder) => GetSettings(builder.Inner);

        /// <summary>
        /// Gets the settings or default settings for a <see cref="IRequestBuilder"/>.
        /// </summary>
        /// <param name="builder">The builder to get settings for.</param>
        public static HttpSettings GetSettings(this IRequestBuilder builder) => builder.Settings ?? Http.DefaultSettings;

        /// <summary>
        /// Adds a key/value pair for logging to an exception, one that'll appear in exceptional
        /// </summary>
        /// <typeparam name="T">The type of exception we're adding to.</typeparam>
        public static T AddLoggedData<T>(this T ex, string key, object value) where T : Exception
        {
            ex.Data[Http.DefaultSettings.ErrorDataPrefix + key] = value?.ToString() ?? "";
            return ex;
        }
    }
}
