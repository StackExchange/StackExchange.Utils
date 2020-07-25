using System;
using Microsoft.Extensions.Configuration;

namespace StackExchange.Utils
{
    /// <summary>
    /// Extension methods for <see cref="IConfigurationBuilder"/> to provide prefixing
    /// and substitution support in <see cref="IConfiguration"/>.
    /// </summary>
    public static class ConfigurationBuilderExtensions
    {
        /// <summary>
        /// Wraps a child <see cref="IConfigurationBuilder"/> with support for substituting values
        /// of the form '${secrets:Database:Password}' with configuration from elsewhere in the
        /// configuration system. The key in braces is looked and used to rewrite the value returned
        /// from configuration. 
        /// </summary>
        public static IConfigurationBuilder WithSubstitution(this IConfigurationBuilder builder, Action<IConfigurationBuilder> action)
        {
            var childBuilder = new ConfigurationBuilder();
            action(childBuilder);
            builder.Add(new SubstitutingConfigurationSource(childBuilder));
            return builder;
        }

        /// <summary>
        /// Wraps a child <see cref="IConfigurationBuilder"/> with support for a prefix on all keys
        /// contained within it. This can be used to effectively namespace a set of configuration values
        /// and is useful when used in combination with <see cref="WithSubstitution"/>.
        /// </summary>
        public static IConfigurationBuilder WithPrefix(this IConfigurationBuilder builder, string prefix, Action<IConfigurationBuilder> action)
        {
            var childBuilder = new ConfigurationBuilder();
            action(childBuilder);
            builder.Add(new PrefixedConfigurationSource(prefix, childBuilder));
            return builder;
        }
    }
}
