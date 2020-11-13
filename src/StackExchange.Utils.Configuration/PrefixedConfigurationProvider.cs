using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace StackExchange.Utils
{
    internal class PrefixedConfigurationProvider : CompositeConfigurationProvider
    {
        private readonly string _prefixWithDelimiter;
        private readonly string _prefix;

        public const char Delimiter = ':';
        
        public PrefixedConfigurationProvider(string prefix, IConfigurationRoot configurationRoot) : base(configurationRoot)
        {
            _prefix = prefix;
            _prefixWithDelimiter = prefix + Delimiter;
        }

        public override IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath)
        {
            if (parentPath != null && !parentPath.StartsWith(_prefix))
            {
                return earlierKeys;
            }

            var keys = new List<string>();
            if (parentPath == null)
            {
                keys.Add(_prefix);
            }
            else
            {
                string parentPathWithoutPrefix = null;
                if (!parentPath.Equals(_prefix, StringComparison.OrdinalIgnoreCase))
                {
                    parentPathWithoutPrefix = WithoutPrefix(parentPath);
                }

                foreach (var provider in ConfigurationRoot.Providers)
                {
                    foreach (var childKey in provider.GetChildKeys(keys, parentPathWithoutPrefix))
                    {
                        keys.Add(childKey);
                    }
                }
            }

            return keys.Concat(earlierKeys).OrderBy(k => k, ConfigurationKeyComparer.Instance);
        }

        public override void Set(string key, string value)
        {
            if (!key.StartsWith(_prefixWithDelimiter) || key.Length == _prefixWithDelimiter.Length)
            {
                return;
            }
            
            base.Set(WithoutPrefix(key), value);
        }
        
        public override bool TryGet(string key, out string value)
        {
            if (!key.StartsWith(_prefixWithDelimiter) || key.Length == _prefixWithDelimiter.Length)
            {
                value = null;
                return false;
            }

            return base.TryGet(WithoutPrefix(key), out value);
        }

        // TODO: make this moar efficient!
        // slice off the prefix so we can fetch from our underlying providers
        private string WithoutPrefix(string path) => path == null ? path : path.AsSpan().Slice(_prefixWithDelimiter.Length).ToString();
    }
}
