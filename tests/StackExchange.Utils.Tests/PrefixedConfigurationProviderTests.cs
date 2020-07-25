using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace StackExchange.Utils.Tests
{
    public class PrefixedConfigurationProviderTests
    {
        [Fact]
        public void ValuesArePrefixed()
        {
            var configuration = new ConfigurationBuilder()
                .WithPrefix(
                    "test",
                    c =>
                    {
                        c.AddInMemoryCollection(
                            new Dictionary<string, string>
                            {
                                ["ShouldBeAccessibleUsingPrefix"] = "Test"
                            });
                    })
                .Build();

            Assert.Null(configuration.GetValue<string>("ShouldBeAccessibleUsingPrefix"));
            Assert.Equal("Test", configuration.GetValue<string>("test:ShouldBeAccessibleUsingPrefix"));
        }
        
        [Fact]
        public void NestedValuesArePrefixed()
        {
            var configuration = new ConfigurationBuilder()
                .WithPrefix(
                    "test",
                    c =>
                    {
                        c.AddInMemoryCollection(
                            new Dictionary<string, string>
                            {
                                ["Nested:ShouldBeAccessibleUsingPrefix"] = "Test"
                            });
                    })
                .Build();

            Assert.Null(configuration.GetValue<string>("ShouldBeAccessibleUsingPrefix"));
            Assert.Equal("Test", configuration.GetValue<string>("test:Nested:ShouldBeAccessibleUsingPrefix"));
        }
        
        [Fact]
        public void JustPrefix()
        {
            var configuration = new ConfigurationBuilder()
                .WithPrefix(
                    "test",
                    c =>
                    {
                        c.AddInMemoryCollection(
                            new Dictionary<string, string>
                            {
                                ["Key"] = "Value"
                            });
                    })
                .Build();

            Assert.Null(configuration.GetValue<string>("test:"));
        }

        [Fact]
        public void NonExistent()
        {
            var configuration = new ConfigurationBuilder()
                .WithPrefix(
                    "test",
                    c =>
                    {
                        c.AddInMemoryCollection(
                            new Dictionary<string, string>
                            {
                                ["Key"] = "Value"
                            });
                    })
                .Build();

            Assert.NotNull(configuration.GetValue<string>("test:Key"));
            Assert.Null(configuration.GetValue<string>("test:NotHere"));
        }

        [Fact]
        public void NestedPrefixes()
        {
            var configuration = new ConfigurationBuilder()
                .WithPrefix(
                    "test",
                    c =>
                    {
                        c.WithPrefix("nested",
                            nested => nested.AddInMemoryCollection(
                                new Dictionary<string, string>
                                {
                                    ["Key"] = "Value"
                                })
                        );
                    })
                .Build();

            Assert.Null(configuration.GetValue<string>("test:Key"));
            Assert.Equal("Value", configuration.GetValue<string>("test:nested:Key"));
        }
    }
}
