using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace StackExchange.Utils.Tests
{
    public class SubstitutionHelperTests
    {
        [Fact]
        public void BasicSubstitution()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new Dictionary<string, string> {["SubstituteKey"] = "SubstituteValue"}
                )
                .Build();
                
            var value = SubstitutionHelper.ReplaceSubstitutionPlaceholders("${SubstituteKey}", configuration);
            Assert.Equal("SubstituteValue", value);
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
                .Build();
                
            var value = SubstitutionHelper.ReplaceSubstitutionPlaceholders(
            "Server=.;Database=Local.Database;User ID=${Database:Username};Password=${Database:Password}", configuration
            );
            Assert.Equal("Server=.;Database=Local.Database;User ID=user;Password=Password1!", value);
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
                .Build();

            var value = SubstitutionHelper.ReplaceSubstitutionPlaceholders("${prefixed:Key}", configuration);
            Assert.Equal("PrefixedValue", value);
        }

        [Fact]
        public void BasicNonExistent()
        {
            var configuration = new ConfigurationBuilder().Build();
            var value = SubstitutionHelper.ReplaceSubstitutionPlaceholders("${prefixed:DoesNotExist}", configuration);
            Assert.Equal("${prefixed:DoesNotExist}", value);
        }
        
        [Fact]
        public void MultipleNonExistent()
        {
            var configuration = new ConfigurationBuilder().Build();
            var value = SubstitutionHelper.ReplaceSubstitutionPlaceholders("Server=.;Database=Local.Database;User ID=${Database:User};Password=${Database:Password}", configuration);
            Assert.Equal("Server=.;Database=Local.Database;User ID=${Database:User};Password=${Database:Password}", value);
        }

        [Fact]
        public void Null()
        {
            var configuration = new ConfigurationBuilder().Build();
            var value = SubstitutionHelper.ReplaceSubstitutionPlaceholders(null, configuration);
            Assert.Null(value);
        }
        
        [Fact]
        public void Empty()
        {
            var configuration = new ConfigurationBuilder().Build();
            var value = SubstitutionHelper.ReplaceSubstitutionPlaceholders("", configuration);
            Assert.Empty(value);
        }
    }
}
