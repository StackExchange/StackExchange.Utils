## Stack Exchange Utility Packages
Home for the StackExchange.Util.* packages.

### StackExchange.Utils.Http
`Http` is a helper class for making `HttpClient` calls. The send, deserialization, options, and verb portions are exchangable. Some examples:

POSTing a string and expecting a string:
```cs
var result = await Http.Request("https://example.com")
                       .SendPlaintext("test")
                       .ExpectString()
                       .PostAsync();
```

POSTing JSON and expecting protobuf back:
```cs
var result = await Http.Request("https://example.com")
                       .SendJson(new { name = "my thing" })
                       .ExpectProtobuf<MyType>()
                       .PostAsync();
```

Sending nothing and GETing JSON back:
```cs
var result = await Http.Request("https://example.com")
                       .ExpectJson<MyType>()
                       .GetAsync();
```

Sending nothing and GETing JSON back, with a timeout, ignoring 404 responses:
```cs
var result = await Http.Request("https://example.com")
                       .IgnoredResponseStatuses(HttpStatusCode.NotFound)
                       .WithTimeout(TimeSpan.FromSeconds(20))
                       .ExpectJson<MyType>()
                       .GetAsync();
// Handle the response:
if (result.Success)
{
    //result.Data is MyType, deserialized from the returned JSON
}
else
{
    // result.Error
    // result.StatusCode
    // result.RawRequest
    // result.RawResponse
}
```

#### Profiling
There are settings (`.ProfileRequest` and `.ProfileGeneral`) specifically for profiling - these are used and disposed around the events. By implementing `IDisposable` in something, you can time the events.

If you're using something like [MiniProfiler](https://miniprofiler.com/dotnet/), you can instrument HTTP calls in the default settings, like this:
```cs
var settings = Http.DefaultSettings;
settings.ProfileRequest = request => MiniProfiler.Current.CustomTiming("http", request.RequestUri.ToString(), request.Method.Method);
settings.ProfileGeneral = name => MiniProfiler.Current.Step(name);
```

### StackExchange.Utils.Configuration
`StackExchange.Utils.Configuration` is a helper library that performs substitution and prefixing for `IConfiguration`-based configuration sources. It allows a value in the configuration tree to reference other values within the configuration system using a placeholder syntax `${key}` or `${nested:key}`. In addition, prefixing allows a "namespace" prefix to be applied to a subset of configuration, making it possible to segment configuration into logical areas.

This is particularly useful for storing secrets in a different, secure location but in a way that it makes it easy to compose configuration values like connection strings without dealing with it inside the application. E.g. consider the following files:

**appsettings.json**
```json
{
    "ConnectionStrings": {
        "Database": "Server=srv01;Database=db01;User ID=${secrets:Database:UserId};Password=${secrets:Database:Password}"
    }
}
```

**secrets.json**
```json
{
    "Database": {
        "UserId": "User123",
        "Password": "Password123!"
    }
}
```

This instructs the substitution provider to lookup the keys `secrets:Database:UserId` and `secrets:Database:Password` from the configuration system and replaces them in the value returned for `ConnectionStrings:Database`.

To support this a `ConfigurationBuilder` is configured as follows:

```c#
var configuration = new ConfigurationBuilder()
    .WithPrefix(
        "secrets",
        // everything in this configuration builder will be prefixed with "secrets:"
        c => c.AddJsonFile("secrets.json")
    )
    .WithSubstitution(
        // values in this configuration builder will have substitutions
        // replaced prior to being returned to the caller
        c => c.AddJsonFile("appsettings.json")
    )
    .Build();
```

Here we're loading a JSON file called `secrets.json` (it could equally be any source supported by the `IConfiguration` system - ideally something secure like Azure KeyVault or Hashicorp's Vault) and prefixing it with `secrets:`. Then, we load `appsettings.json` with support for substitutions. If a caller asks the `IConfiguration` that is produced for a connection string:

```c#
var connectionString = configuration.GetConnectionString("Database");
```

That value will be returned as `Server=srv01;Database=db01;User ID=User123;Password=Password123!`.

#### Substituting existing values
In some cases it's useful to be able to substitute placeholders in existing strings. Support for that is provided by the `SubstitutionHelper`:

```c#
var configuration = new ConfigurationBuilder()
    .WithPrefix(
        "secrets",
        // everything in this configuration builder will be prefixed with "secrets:"
        c => c.AddJsonFile("secrets.json")
    )
    .Build();

var value = "Server=srv01;Database=db01;User ID=${secrets:Database:UserId};Password=${secrets:Database:Password}";
var valueWithSubstitution = SubstitutionHelper.ReplaceSubstitutionPlaceholders(value, configuration);
```

This will do exactly the same as if the value was substituted within the configuration system itself - the returned value will be `Server=srv01;Database=db01;User ID=User123;Password=Password123!`.

#### Notes
If a value has substitution placeholders that could not be replaced they are left intact - only placeholder keys that can be located in the configuration system are replaced.

### License
StackExchange.Utils is licensed under the [MIT license](https://github.com/StackExchange/StackExchange.Utils/blob/master/LICENSE.txt).
