using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace StackExchange.Utils
{
    internal class PrefixedConfigurationProvider : CompositeConfigurationProvider
    {
        private readonly string _prefixWithDelimiter;
        public const char Delimiter = ':';
        
        public PrefixedConfigurationProvider(string prefix, IConfigurationRoot configurationRoot) : base(configurationRoot)
        {
            _prefixWithDelimiter = prefix + Delimiter;
        }

        public override IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath)
        {
            var earlierKeyList = earlierKeys.ToList();
            foreach (var provider in ConfigurationRoot.Providers)
            {
                foreach (var childKey in provider.GetChildKeys(earlierKeyList, parentPath))
                {
                    yield return _prefixWithDelimiter + childKey;
                }
            }
        }

        public override bool TryGet(string key, out string value)
        {
            if (!key.StartsWith(_prefixWithDelimiter) || key.Length == _prefixWithDelimiter.Length)
            {
                value = null;
                return false;
            }

            // TODO: make this moar efficient!
            // slice off the prefix so we can fetch from our underlying providers
            var keyWithoutPrefix = key.AsSpan().Slice(_prefixWithDelimiter.Length);
            return base.TryGet(keyWithoutPrefix.ToString(), out value);
        }
    }
}
