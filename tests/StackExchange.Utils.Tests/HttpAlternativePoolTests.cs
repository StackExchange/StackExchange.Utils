using System.Net;
using System.Net.Http;
using Xunit;

namespace StackExchange.Utils.Tests
{
    public class HttpAlternativePoolTests
    {
        [Fact]
        public async System.Threading.Tasks.Task UseAlternativePool()
        {
            var customPool = new CountingPool(Http.DefaultSettings);
            var request = Http.Request("https://httpbin.org/get")
                              .WithClientPool(customPool);

            Assert.Equal(customPool, request.ClientPool);

            var result = await request.ExpectJson<HttpBinResponse>()
                                      .GetAsync();

            Assert.Equal(1, customPool.GetCount);
            Assert.True(result.Success);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result);
            Assert.Equal("https://httpbin.org/get", result.Data.Url);
            Assert.Equal(Http.DefaultSettings.UserAgent, result.Data.Headers["User-Agent"]);
        }
    }

    public class CountingPool : IHttpClientPool
    {
        private DefaultHttpClientPool InnerPool { get; }
        public int GetCount { get; private set; } = 0;

        public CountingPool(HttpSettings settings)
        {
            InnerPool = new DefaultHttpClientPool(settings);
        }

        public HttpClient Get(IRequestBuilder builder)
        {
            GetCount++;
            return InnerPool.Get(builder);
        }

        public void Clear() => InnerPool.Clear();
    }
}
