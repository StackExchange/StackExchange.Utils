using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace StackExchange.Utils
{
    /// <summary>
    /// Helper class that replaces substitution values in a string from <see cref="IConfiguration"/>.
    /// </summary>
    public static class SubstitutionHelper
    {
        // matches a key wrapped in braces and prefixed with a '$' 
        // e.g. ${Key} or ${Section:Key} or ${Section:NestedSection:Key}
        private static readonly Regex _substitutionPattern = new(
            @"\$\{(?<key>[^\s]+?)\}", RegexOptions.Compiled
        );
        
        private static readonly ThreadLocal<ICycleDetector> _cycleDetector = new();

        /// <summary>
        /// Replaces substitution placeholders in a value from configuration
        /// from the provided <see cref="IConfiguration"/> containing values.
        /// </summary>
        /// <param name="configuration">
        /// An <see cref="IConfiguration"/> used to locate the value of <paramref name="key"/>. 
        /// </param>
        /// <param name="key">The configuration key.</param>
        /// <param name="value">
        /// Value of the key, if found. If it has substitution placeholders it will be returned with them replaced.
        /// </param>
        /// <param name="substitutions">
        /// An <see cref="IConfiguration"/> used as the source of substitution values.
        /// </param>
        /// <returns>
        /// <c>true</c> if the key was found, <c>false</c> otherwise.
        /// </returns>
        internal static bool TryGetWithSubstitution(this IConfiguration configuration, string key, out string value, IConfiguration substitutions)
        {
            value = configuration[key];
            if (value == null)
            {
                return false;
            }

            value = ReplaceSubstitutionPlaceholders(key, value, substitutions, () => new CycleDetector());
            return true;
        }
        
        /// <summary>
        /// Replaces substitution placeholders in the specified string with
        /// values in the provided <see cref="IConfiguration"/>.
        /// </summary>
        /// <param name="value">
        /// A string containing 0 or more substitution placeholders.
        /// </param>
        /// <param name="substitutions">
        /// An <see cref="IConfiguration"/> used as the source of substitution values.
        /// </param>
        /// <returns>
        /// A string with substitutions replaced or unmodified if none were found.
        /// </returns>
        public static string ReplaceSubstitutionPlaceholders(string value, IConfiguration substitutions)
        {
            return ReplaceSubstitutionPlaceholders(null, value, substitutions, () => NoOpCycleDetector.Instance);
        }
        
        private static string ReplaceSubstitutionPlaceholders(string key, string value, IConfiguration substitutions, Func<ICycleDetector> cycleDetectorFactory)
        {
            if (value == null)
            {
                return null;
            }
            
            // does the value have substitution placeholders in it?
            var matches = _substitutionPattern.Matches(value);
            if (matches.Count > 0)
            {
                // detect cycles - this can happen when there are multiple
                // substitutions that reference values in other child providers.
                // This will end up overflowing the stack so we detect it here
                // and throw an exception
                var cycleDetector = _cycleDetector.Value ??= cycleDetectorFactory();
                
                cycleDetector.Add(key);
                
                // we have some things to substitute, so manipulate the value as a span
                // so we can slice and dice the substitution values
                var valueBuilder = new StringBuilder(value.Length);
                var valueSpan = value.AsSpan();
                var index = 0;

                try
                {
                    foreach (Match match in matches)
                    {
                        var substitutionKey = match.Groups["key"].Value;
                        if (cycleDetector.Contains(substitutionKey))
                        {
                            ThrowCycleDetectedError(key, substitutionKey);
                        }

                        var substitutionValue = substitutions[substitutionKey];
                        if (substitutionValue != null)
                        {
                            // rewrite the value with the substitution
                            valueBuilder.Append(
                                valueSpan.Slice(index, match.Index - index)
                            );
                            
                            valueBuilder.Append(substitutionValue);
                            index = match.Index + match.Length;
                        }
                    }
                }
                finally
                {
                    cycleDetector.Remove(key);
                }
                
                // copy across the remainder of the string
                valueBuilder.Append(
                    valueSpan.Slice(index, value.Length - index)
                );

                value = valueBuilder.ToString();
            }

            return value;
        }
        
        private static void ThrowCycleDetectedError(string key, string substitutionKey) => throw new InvalidOperationException(
            $"Cycle detected for when trying to substitute [{substitutionKey}] in [{key}]"
        );
        
        private class CycleDetector : ICycleDetector
        {
            private readonly HashSet<string> _values = new(StringComparer.OrdinalIgnoreCase);

            public void Add(string key) => _values.Add(key);

            public void Remove(string key) => _values.Remove(key);

            public bool Contains(string key) => _values.Contains(key);
        }

        private class NoOpCycleDetector : ICycleDetector
        {
            public static readonly ICycleDetector Instance = new NoOpCycleDetector();

            private NoOpCycleDetector() { }

            public void Add(string key) { }

            public void Remove(string key) { }

            public bool Contains(string key) => false;
        }
    
        private interface ICycleDetector
        {
            void Add(string key);
            void Remove(string key);
            bool Contains(string key);
        }
    }
}
