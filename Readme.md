## Stack Exchange Utility Packages
Home for the StackExchange.Util.* packages.

#### StackExchange.Utils.Http
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

#### License
MiniProfiler is licensed under the [MIT license](https://github.com/StackExchange/StackExchange.Utils/blob/master/LICENSE.txt).