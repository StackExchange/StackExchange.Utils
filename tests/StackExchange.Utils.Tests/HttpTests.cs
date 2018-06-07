using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
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
        public async Task BasicGet()
        {
            var guid = Guid.NewGuid().ToString();
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
        public async Task BasicDelete()
        {
            var guid = Guid.NewGuid().ToString();
            var result = await Http.Request("https://httpbin.org/delete")
                                    .SendPlaintext(guid)
                                    .ExpectJson<HttpBinResponse>()
                                    .DeleteAsync();
            Assert.True(result.Success);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.True(result.Data.Form.ContainsKey(guid));
            Assert.Equal("https://httpbin.org/delete", result.Data.Url);
            Assert.Equal(Http.DefaultSettings.UserAgent, result.Data.Headers["User-Agent"]);
        }

        private class HttpBinResponse
        {
            [DataMember(Name = "args")]
            public Dictionary<string, string> args { get; set; }

            [DataMember(Name = "data")]
            public string Data { get; set; }

            [DataMember(Name = "files")]
            public Dictionary<string, string> files { get; set; }

            [DataMember(Name = "form")]
            public Dictionary<string, string> Form { get; set; }

            [DataMember(Name = "headers")]
            public Dictionary<string, string> Headers { get; set; }

            [DataMember(Name = "json")]
            public object JSON { get; set; }

            [DataMember(Name = "origin")]
            public string Origin { get; set; }

            [DataMember(Name = "url")]
            public string Url { get; set; }
        }
    }
}
