using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace StackExchange.Utils.Tests
{
    public class SubstitutingConfigurationProviderTests
    {
        [Fact]
        public void BasicSubstitution()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new Dictionary<string, string>
                    {
                        ["SubstituteKey"] = "SubstituteValue"
                    }
                )
                .WithSubstitution(
                    c =>
                    {
                        c.AddInMemoryCollection(
                            new Dictionary<string, string>
                            {
                                ["Key"] = "${SubstituteKey}"
                            }
                        );
                    })
                .Build();
            
            Assert.Equal("SubstituteValue", configuration.GetValue<string>("Key"));
        }
        
        [Fact]
        public void MultipleSubstitution()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new Dictionary<string, string>
                    {
                        ["Database:Username"] = "user",
                        ["Database:Password"] = "Password1!"
                    }
                )
                .WithSubstitution(
                    c =>
                    {
                        c.AddInMemoryCollection(
                            new Dictionary<string, string>
                            {
                                ["ConnectionStrings:Database"] = "Server=.;Database=Local.Database;User ID=${Database:Username};Password=${Database:Password}"
                            }
                        );
                    })
                .Build();
            
            Assert.Equal("Server=.;Database=Local.Database;User ID=user;Password=Password1!", configuration.GetConnectionString("Database"));
        }
        
        [Fact]
        public void PrefixSubstitution()
        {
            var configuration = new ConfigurationBuilder()
                .WithPrefix(
                    "prefixed",
                    prefixed =>
                    {
                        prefixed.AddInMemoryCollection(
                            new Dictionary<string, string>
                            {
                                ["Key"] = "PrefixedValue"
                            }
                        );
                    }
                )
                .WithSubstitution(
                    c =>
                    {
                        c.AddInMemoryCollection(
                            new Dictionary<string, string>
                            {
                                ["Key"] = "${prefixed:Key}"
                            }
                        );
                    })
                .Build();
            
            Assert.Equal("PrefixedValue", configuration.GetValue<string>("Key"));
        }

        [Fact]
        public void BasicNonExistent()
        {
            var configuration = new ConfigurationBuilder()
                .WithSubstitution(
                    c =>
                    {
                        c.AddInMemoryCollection(
                            new Dictionary<string, string>
                            {
                                ["Key"] = "${prefixed:DoesNotExist}"
                            }
                        );
                    })
                .Build();
            
            Assert.Equal("${prefixed:DoesNotExist}", configuration.GetValue<string>("Key"));
        }
        
        [Fact]
        public void MultipleNonExistent()
        {
            var configuration = new ConfigurationBuilder()
                .WithSubstitution(
                    c =>
                    {
                        c.AddInMemoryCollection(
                            new Dictionary<string, string>
                            {
                                ["Key"] = "Server=.;Database=Local.Database;User ID=${Database:User};Password=${Database:Password}"
                            }
                        );
                    })
                .Build();
            
            Assert.Equal("Server=.;Database=Local.Database;User ID=${Database:User};Password=${Database:Password}", configuration.GetValue<string>("Key"));
        }
        
        [Fact]
        public void CyclesAreDetected()
        {
            var configuration = new ConfigurationBuilder()
                .WithSubstitution(
                    c =>
                    {
                        c.AddInMemoryCollection(
                            new Dictionary<string, string>
                            {
                                ["KeyA"] = "${KeyB}"
                            }
                        );
                    }
                )
                .WithSubstitution(
                    c =>
                    {
                        c.AddInMemoryCollection(
                            new Dictionary<string, string>
                            {
                                ["KeyB"] = "${KeyA}"
                            }
                        );
                    })
                .Build();

            Assert.Throws<InvalidOperationException>(
                () => configuration.GetValue<string>("KeyA")
            );
        }
        
        [Fact]
        public void DoesNotCycleInSameSubstitutionSource()
        {
            var configuration = new ConfigurationBuilder()
                .WithSubstitution(
                    c =>
                    {
                        c.AddInMemoryCollection(
                            new Dictionary<string, string>
                            {
                                ["KeyA"] = "${KeyB}",
                                ["KeyB"] = "${KeyA}"
                            }
                        );
                    })
                .Build();

            Assert.Equal("${KeyB}", configuration.GetValue<string>("KeyA"));
            Assert.Equal("${KeyA}", configuration.GetValue<string>("KeyB"));
        }
        
        [Fact]
        public void MultiHopCyclesAreDetected()
        {
            var configuration = new ConfigurationBuilder()
                .WithSubstitution(
                    c =>
                    {
                        c.AddInMemoryCollection(
                            new Dictionary<string, string>
                            {
                                ["KeyA"] = "${KeyB}"
                            }
                        );
                    }
                )
                .WithSubstitution(
                    c =>
                    {
                        c.AddInMemoryCollection(
                            new Dictionary<string, string>
                            {
                                ["KeyB"] = "${KeyC}"
                            }
                        );
                    })
                .WithSubstitution(
                    c =>
                    {
                        c.AddInMemoryCollection(
                            new Dictionary<string, string>
                            {
                                ["KeyC"] = "${KeyA}"
                            }
                        );
                    })
                .Build();

            Assert.Throws<InvalidOperationException>(
                () => configuration.GetValue<string>("KeyA")
            );
        }
    }
}
