using System;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace StackExchange.Utils
{
    internal class SubstitutingConfigurationSource : IConfigurationSource
    {
        private readonly IConfigurationBuilder _childBuilder;

        public SubstitutingConfigurationSource(ConfigurationBuilder childBuilder)
        {
            _childBuilder = childBuilder;
        }
        
        private static readonly IConfigurationRoot _emptyRoot = new ConfigurationRoot(Array.Empty<IConfigurationProvider>());

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            var substitutionRoot = _childBuilder.Build();
            if (builder.Sources.Count - 1 == 0)
            {
                // if we're the only source, there's nothing to replace
                return new SubstitutingConfigurationProvider(
                    _emptyRoot, substitutionRoot
                );
            }

            var valueBuilder = new ConfigurationBuilder();
            for (var i = 0; i < builder.Sources.Count; i++)
            {
                var source = builder.Sources[i];
                if (!ReferenceEquals(source, this))
                {
                    valueBuilder.Add(source);
                }
            }

            var valueRoot = valueBuilder.Build();
            return new SubstitutingConfigurationProvider(
                valueRoot, substitutionRoot
            );
        }
    }
}
