using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace StackExchange.Utils
{
    internal abstract class CompositeConfigurationProvider : IConfigurationProvider
    {
        protected CompositeConfigurationProvider(IConfigurationRoot configurationRoot)
        {
            ConfigurationRoot = configurationRoot;
        }

        protected IConfigurationRoot ConfigurationRoot { get; }

        public virtual IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath)
            => ConfigurationRoot.Providers.SelectMany(x => x.GetChildKeys(earlierKeys, parentPath));
        
        public IChangeToken GetReloadToken() => new CompositeChangeToken(
            ConfigurationRoot.Providers.Select(x => x.GetReloadToken()).ToImmutableArray()
        );

        private bool _initialLoad = true;
        public void Load()
        {
            if (_initialLoad)
            {
                _initialLoad = false;
                return;
            }
            ConfigurationRoot.Reload();
        }

        public virtual void Set(string key, string value) => ConfigurationRoot[key] = value;

        public virtual bool TryGet(string key, out string value)
        {
            value = ConfigurationRoot[key];
            return value != null;
        }
    }
}
