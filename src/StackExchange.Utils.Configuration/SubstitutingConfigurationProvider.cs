using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace StackExchange.Utils
{
    class SubstitutingConfigurationProvider : CompositeConfigurationProvider
    {
        private readonly IConfigurationRoot _valueRoot;

        public SubstitutingConfigurationProvider(IConfigurationRoot valueRoot, IConfigurationRoot configurationRoot) : base(configurationRoot)
        {
            _valueRoot = valueRoot;
        }

        public override bool TryGet(string key, out string value) => ConfigurationRoot.TryGetWithSubstitution(key, out value, _valueRoot);
    }
}
