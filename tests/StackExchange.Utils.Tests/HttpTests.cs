using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace StackExchange.Utils.Tests
{
    public class HttpTests
    {
        [Fact]
        public async Task BasicCreation()
        {
            var request = Http.Request("https://example.com/")
                              .SendPlaintext("test")
                              .ExpectString();
            Assert.Equal("https://example.com/", request.Inner.Message.RequestUri.ToString());
            var content = Assert.IsType<StringContent>(request.Inner.Message.Content);
            var stringContent = await content.ReadAsStringAsync();
            Assert.Equal("test", stringContent);
        }

        [Fact]
        public async Task BasicDelete()
        {
            var result = await Http.Request("https://httpbin.org/delete")
                                    .ExpectJson<HttpBinResponse>()
                                    .DeleteAsync();
            Assert.True(result.Success);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("https://httpbin.org/delete", result.Data.Url);
            Assert.Equal(Http.DefaultSettings.UserAgent, result.Data.Headers["User-Agent"]);
        }

        [Fact]
        public async Task BasicGet()
        {
            var result = await Http.Request("https://httpbin.org/get")
                                    .ExpectJson<HttpBinResponse>()
                                    .GetAsync();
            Assert.True(result.Success);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result);
            Assert.Equal("https://httpbin.org/get", result.Data.Url);
            Assert.Equal(Http.DefaultSettings.UserAgent, result.Data.Headers["User-Agent"]);
        }

        [Fact]
        public async Task BasicHead()
        {
            var guid = Guid.NewGuid().ToString();
            var result = await Http.Request("https://httpbin.org/anything")
                                    .SendPlaintext(guid)
                                    .ExpectJson<HttpBinResponse>()
                                    .PutAsync();
            Assert.True(result.Success);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.True(result.Data.Form.ContainsKey(guid));
            Assert.Equal("https://httpbin.org/anything", result.Data.Url);
            Assert.Equal(Http.DefaultSettings.UserAgent, result.Data.Headers["User-Agent"]);
        }

        [Fact]
        public async Task BasicPost()
        {
            var guid = Guid.NewGuid().ToString();
            var result = await Http.Request("https://httpbin.org/post")
                                    .SendPlaintext(guid)
                                    .ExpectJson<HttpBinResponse>()
                                    .PostAsync();
            Assert.True(result.Success);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result);
            Assert.True(result.Data.Form.ContainsKey(guid));
            Assert.Equal("https://httpbin.org/post", result.Data.Url);
            Assert.Equal(Http.DefaultSettings.UserAgent, result.Data.Headers["User-Agent"]);
        }

        [Fact]
        public async Task BasicPut()
        {
            var guid = Guid.NewGuid().ToString();
            var result = await Http.Request("https://httpbin.org/put")
                                    .SendPlaintext(guid)
                                    .ExpectJson<HttpBinResponse>()
                                    .PutAsync();
            Assert.True(result.Success);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.True(result.Data.Form.ContainsKey(guid));
            Assert.Equal("https://httpbin.org/put", result.Data.Url);
            Assert.Equal(Http.DefaultSettings.UserAgent, result.Data.Headers["User-Agent"]);
        }

        [Fact]
        public async Task ErrorIgnores()
        {
            var settings = new HttpSettings();
            var errorCount = 0;
            settings.Exception += (_, __) => errorCount++;

            var result = await Http.Request("https://httpbin.org/satus/404", settings)
                                   .ExpectHttpSuccess()
                                   .GetAsync();
            Assert.False(result.Success);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.Equal(1, errorCount);

            result = await Http.Request("https://httpbin.org/satus/404", settings)
                               .WithoutLogging(HttpStatusCode.NotFound)
                               .ExpectHttpSuccess()
                               .GetAsync();
            Assert.False(result.Success);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.Equal(1, errorCount); // didn't go up

            result = await Http.Request("https://httpbin.org/satus/404", settings)
                               .WithoutErrorLogging()
                               .ExpectHttpSuccess()
                               .GetAsync();
            Assert.False(result.Success);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.Equal(1, errorCount); // didn't go up
        }

        [Fact]
        public async Task Timeouts()
        {
            var result = await Http.Request("https://httpbin.org/delay/10")
                                   .WithTimeout(TimeSpan.FromSeconds(1))
                                   .ExpectHttpSuccess()
                                   .GetAsync();
            Assert.False(result.Success);
            Assert.NotNull(result.Error);
            Assert.Equal("HttpClient request timed out. Timeout: 1,000ms", result.Error.Message);
            var err = Assert.IsType<HttpClientException>(result.Error);
            Assert.Equal("https://httpbin.org/delay/10", err.Uri.ToString());
            Assert.Null(err.StatusCode);
        }

        [Fact]
        public async Task AddHeaderWithoutValidation()
        {
            var result = await Http.Request("https://httpbin.org/bearer")
                                   .AddHeaderWithoutValidation("Authorization", "abcd")
                                   .ExpectJson<HttpBinResponse>()
                                   .GetAsync();
            Assert.True(result.RawRequest.Headers.Contains("Authorization"));
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task AddHeaders()
        {
            var result = await Http.Request("https://httpbin.org/headers")
                .AddHeaders(new Dictionary<string, string>()
                {
                    {"Content-Type", "application/json"},
                    {"Custom", "Test"}
                })
                .SendJson("{}")
                .ExpectJson<HttpBinResponse>()
                .GetAsync();

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("Test", result.Data.Headers["Custom"]);
            // Content-Type should be present because we're sending a body up
            Assert.StartsWith("application/json", result.Data.Headers["Content-Type"]);
        }

        [Fact]
        public async Task TestAddHeadersWhereClientAddsHeaderBeforeContent()
        {
            var result = await Http.Request("https://httpbin.org/headers")
                .AddHeaders(new Dictionary<string, string>()
                {
                    {"Content-Type", "application/json"},
                    {"Custom", "Test"}
                })
                .SendJson("")
                .ExpectJson<HttpBinResponse>()
                .GetAsync();

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("Test", result.Data.Headers["Custom"]);
            Assert.Equal("application/json; charset=utf-8", result.Data.Headers["Content-Type"]);
        }

        [Fact]
        public async Task TestAddHeadersWhereClientAddsHeaderAndNoContent()
        {
            var result = await Http.Request("https://httpbin.org/headers")
                .AddHeaders(new Dictionary<string, string>()
                {
                    {"Content-Type", "application/json"},
                    {"Custom", "Test"}
                })
                .ExpectJson<HttpBinResponse>()
                .GetAsync();

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("Test", result.Data.Headers["Custom"]);
            // Content-Type is NOT sent when there's no body - this is correct behavior
            Assert.DoesNotContain("Content-Type", result.Data.Headers.Keys);
        }

        [Fact]
        public async Task PatchRequest()
        {
            var guid = Guid.NewGuid().ToString();
            var result = await Http.Request("https://httpbin.org/patch")
                .SendPlaintext(guid)
                .ExpectJson<HttpBinResponse>()
                .PatchAsync();

            Assert.True(result.Success);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result);
            Assert.True(result.Data.Form.ContainsKey(guid));
            Assert.Equal("https://httpbin.org/patch", result.Data.Url);
            Assert.Equal(Http.DefaultSettings.UserAgent, result.Data.Headers["User-Agent"]);
        }

        [Fact]
        public async Task SendFormUsesMultipartFormData()
        {
            string expectedFormKey = "key";
            string expectedFormValue = "value";
            var form = new NameValueCollection
            {
                [expectedFormKey] = expectedFormValue
            };
            var result = await Http.Request("https://httpbin.org/post")
                .SendForm(form)
                .ExpectJson<HttpBinResponse>()
                .PostAsync();

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var actualContentType = Assert.Contains("Content-Type", (IDictionary<string, string>)result.Data.Headers);
            Assert.StartsWith("multipart/form-data;", actualContentType);
            var actualFormValue = Assert.Contains(expectedFormKey, (IDictionary<string, string>)result.Data.Form);
            Assert.Equal(expectedFormValue, actualFormValue);
        }

        [Fact]
        public async Task SendFormUrlEncodedUsesFormDataUrlEncoded()
        {
            string expectedFormKey = "key";
            string expectedFormValue = "value";
            var form = new NameValueCollection
            {
                [expectedFormKey] = expectedFormValue
            };
            var result = await Http.Request("https://httpbin.org/post")
                .SendFormUrlEncoded(form)
                .ExpectJson<HttpBinResponse>()
                .PostAsync();

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var actualContentType = Assert.Contains("Content-Type", (IDictionary<string, string>)result.Data.Headers);
            Assert.Equal("application/x-www-form-urlencoded", actualContentType);
            var actualFormValue = Assert.Contains(expectedFormKey, (IDictionary<string, string>)result.Data.Form);
            Assert.Equal(expectedFormValue, actualFormValue);
        }

        [Fact]
        public async Task LargePost()
        {
            // 5MB string
            var myString = new string('*', 1048576 * 5);

            var form = new System.Collections.Specialized.NameValueCollection
            {
                ["requestsJson"] = myString
            };

            var result = await Http.Request("https://httpbin.org/post")
                .SendForm(form)
                .ExpectJson<HttpBinResponse>()
                .PostAsync();

            Assert.True(result.Success);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result);
            Assert.Equal("https://httpbin.org/post", result.Data.Url);
            Assert.Equal(Http.DefaultSettings.UserAgent, result.Data.Headers["User-Agent"]);
        }

        [Fact]
        public async Task NullSendFormValue()
        {
            var settings = new HttpSettings();

            var form = new System.Collections.Specialized.NameValueCollection
            {
                ["nullValue"] = null,
            };

            var result = await Http.Request("https://httpbin.org/post", settings)
                .SendForm(form)
                .ExpectJson<HttpBinResponse>()
                .PostAsync();

            Assert.True(result.Success);
            Assert.Null(result.Error);
            Assert.Same("", result.Data.Form["nullValue"]);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void NullSendFormKey(string key)
        {
            var settings = new HttpSettings();

            var form = new System.Collections.Specialized.NameValueCollection
            {
                [key] = "nullKey",
                // if not handled, throws System.ArgumentException
                // at System.Net.Http.MultipartFormDataContent.Add(HttpContent content, String name)
                // at StackExchange.Utils.ExtensionsForHttp.SendForm(IRequestBuilder builder, NameValueCollection form)
            };

            Assert.ThrowsAny<Exception>(() => Http.Request("https://httpbin.org/post", settings).SendForm(form));
            // TODO would it be more appropriate to return just log the error and return it in result.Error after .PostAsync?
        }
    }
}
