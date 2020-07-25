using Microsoft.Extensions.Configuration;

namespace StackExchange.Utils
{
    internal class PrefixedConfigurationSource : IConfigurationSource
    {
        private readonly IConfigurationBuilder _childBuilder;
        private readonly string _prefix;

        public PrefixedConfigurationSource(string prefix, ConfigurationBuilder childBuilder)
        {
            _childBuilder = childBuilder;
            _prefix = prefix;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder) => new PrefixedConfigurationProvider(_prefix, _childBuilder.Build());
    }
}
