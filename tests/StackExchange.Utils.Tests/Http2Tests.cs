#if KESTREL
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using RunTestServer;
using Xunit;
using Xunit.Abstractions;

namespace StackExchange.Utils.Tests
{
    public class Http2Tests : IClassFixture<Http2Server>
    {
        private readonly Http2Server _server;
        private readonly ITestOutputHelper _log;
        private void Log(string message) => _log.WriteLine(message);
        public Http2Tests(ITestOutputHelper log, Http2Server server)
        {
            _server = server ?? throw new ArgumentNullException(nameof(server));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public enum ServerMode
        {
            Http1Only,
            Http2Only,
            Http1or2,
            Https1or2,
        }

        [Theory]
        // non-TLS http1: should work (returning 1.1)
        [InlineData(ServerMode.Http1Only, false, null, "1.1", "HTTP/1.1")]
        [InlineData(ServerMode.Http1Only, false, "1.1", "1.1", "HTTP/1.1")]
        [InlineData(ServerMode.Http1Only, false, "2.0", "1.1", "HTTP/1.1")]

        // non-TLS http2 without the global override: should always fail
        [InlineData(ServerMode.Http2Only, false, null, "1.1", "HTTP/1.1", true)]
        [InlineData(ServerMode.Http2Only, false, "1.1", "1.1", "HTTP/1.1", true)]
        [InlineData(ServerMode.Http2Only, false, "2.0", "2.0", "HTTP/2", true)]

        // non-TLS http2 with the global override: should work if we specify http2
        [InlineData(ServerMode.Http2Only, true, null, "1.1", "HTTP/1.1", true)]
        [InlineData(ServerMode.Http2Only, true, "1.1", "1.1", "HTTP/1.1", true)]
        [InlineData(ServerMode.Http2Only, true, "2.0", "2.0", "HTTP/2")]

        // non-TLS http* without the global override: should work, server prefers 1.1
        [InlineData(ServerMode.Http1or2, false, null, "1.1", "HTTP/1.1")]
        [InlineData(ServerMode.Http1or2, false, "1.1", "1.1", "HTTP/1.1")]
        [InlineData(ServerMode.Http1or2, false, "2.0", "1.1", "HTTP/1.1")]

        // non-TLS http* with the global override: should work for 1.1; with 2, client and server argue
        [InlineData(ServerMode.Http1or2, true, null, "1.1", "HTTP/1.1")]
        [InlineData(ServerMode.Http1or2, true, "1.1", "1.1", "HTTP/1.1")]
        [InlineData(ServerMode.Http1or2, true, "2.0", "2.0", "HTTP/2", true)]

        // TLS http*: should always work
        [InlineData(ServerMode.Https1or2, false, null, "1.1", "HTTP/1.1")]
        [InlineData(ServerMode.Https1or2, false, "1.1", "1.1", "HTTP/1.1")]
        [InlineData(ServerMode.Https1or2, false, "2.0", "2.0", "HTTP/2")]
        public async Task UsesVersion(ServerMode mode, bool allowUnencryptedHttp2, string specified, string expectedVersion, string expectedResponse, bool failure = false)
        {
            bool oldMode = Http.AllowUnencryptedHttp2;
            try
            {
                Http.AllowUnencryptedHttp2 = allowUnencryptedHttp2;

                var uri = mode switch
                {
                    ServerMode.Http1Only => Http2Server.Http1Uri,
                    ServerMode.Http2Only => Http2Server.Http2Uri,
                    ServerMode.Http1or2 => Http2Server.Http1or2Uri,
                    ServerMode.Https1or2 => Http2Server.Https1or2Uri,
                    _ => throw new ArgumentOutOfRangeException(nameof(mode)),
                };
                Log($"Server is on {uri}; specifying: '{specified}'");
                var request = Http.Request(uri, new HttpSettings {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });
                if (specified is object) request = request.WithProtocolVersion(new Version(specified));
                HttpCallResponse<string> result;
                try
                {
                    result = await request.ExpectString().GetAsync();
                }
                catch(Exception ex)
                {
                    if (failure)
                    {
                        Log(ex.ToString());
                        return;
                    }
                    else
                    {
                        throw;
                    }
                }

                Log($"As sent: {result?.RawRequest?.Version}, received: {result?.RawResponse?.Version}");

                if (failure)
                {
                    Assert.NotNull(result.Error);
                    Log(result.Error.ToString());
                }
                else
                {
                    Assert.Null(result.Error);
                    Assert.Equal(expectedVersion, result.RawResponse?.Version?.ToString());
                    Assert.Equal(expectedResponse, result.Data);
                }
                
            }
            finally
            {
                Http.AllowUnencryptedHttp2 = oldMode;
            }
        }
    }
}
#endif
