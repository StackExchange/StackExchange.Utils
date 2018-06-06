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
            var request = Http.Request("https://google.com/")
                              .SendPlaintext("test")
                              .ExpectString();
            Assert.Equal("https://google.com/", request.Inner.Message.RequestUri.ToString());
            var content = Assert.IsType<StringContent>(request.Inner.Message.Content);
            var stringContent = await content.ReadAsStringAsync();
            Assert.Equal("test", stringContent);
        }
    }
}
