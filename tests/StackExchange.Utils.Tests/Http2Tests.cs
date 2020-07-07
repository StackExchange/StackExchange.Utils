#if KESTREL
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Xunit;
using Xunit.Abstractions;

namespace StackExchange.Utils.Tests
{
    public class Http2Tests : IClassFixture<Http2Tests.Http2Server>
    {
        private readonly Http2Server _server;
        private readonly ITestOutputHelper _log;
        private void Log(string message) => _log.WriteLine(message);
        public Http2Tests(ITestOutputHelper log, Http2Server server)
        {
            _server = server ?? throw new ArgumentNullException(nameof(server));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        [Theory]
        // non-TLS http1: should work (returning 1.1)
        [InlineData(HttpProtocols.Http1, false, false, null, "1.1", "HTTP/1.1")]
        [InlineData(HttpProtocols.Http1, false, false, "1.1", "1.1", "HTTP/1.1")]
        [InlineData(HttpProtocols.Http1, false, false, "2.0", "1.1", "HTTP/1.1")]

        // non-TLS http2 without the global override: should always fail
        [InlineData(HttpProtocols.Http2, false, false, null, "1.1", "HTTP/1.1", true)]
        [InlineData(HttpProtocols.Http2, false, false, "1.1", "1.1", "HTTP/1.1", true)]
        [InlineData(HttpProtocols.Http2, false, false, "2.0", "2.0", "HTTP/2", true)]

        // non-TLS http2 with the global override: should work if we specify http2
        [InlineData(HttpProtocols.Http2, false, true, null, "1.1", "HTTP/1.1", true)]
        [InlineData(HttpProtocols.Http2, false, true, "1.1", "1.1", "HTTP/1.1", true)]
        [InlineData(HttpProtocols.Http2, false, true, "2.0", "2.0", "HTTP/2")]

        // non-TLS http* without the global override: should work, server prefers 1.1
        [InlineData(HttpProtocols.Http1AndHttp2, false, false, null, "1.1", "HTTP/1.1")]
        [InlineData(HttpProtocols.Http1AndHttp2, false, false, "1.1", "1.1", "HTTP/1.1")]
        [InlineData(HttpProtocols.Http1AndHttp2, false, false, "2.0", "1.1", "HTTP/1.1")]

        // non-TLS http* with the global override: should work for 1.1; with 2, client and server argue
        [InlineData(HttpProtocols.Http1AndHttp2, false, true, null, "1.1", "HTTP/1.1")]
        [InlineData(HttpProtocols.Http1AndHttp2, false, true, "1.1", "1.1", "HTTP/1.1")]
        [InlineData(HttpProtocols.Http1AndHttp2, false, true, "2.0", "2.0", "HTTP/2", true)]

        // TLS http1: should always work, but http2 attempt is ignored
        [InlineData(HttpProtocols.Http1, true, false, null, "1.1", "HTTP/1.1")]
        [InlineData(HttpProtocols.Http1, true, false, "1.1", "1.1", "HTTP/1.1")]
        [InlineData(HttpProtocols.Http1, true, false, "2.0", "1.1", "HTTP/1.1")]

        // TLS http2: should work as long as we actually send http2
        [InlineData(HttpProtocols.Http2, true, false, null, "1.1", "HTTP/1.1", true)]
        [InlineData(HttpProtocols.Http2, true, false, "1.1", "1.1", "HTTP/1.1", true)]
        [InlineData(HttpProtocols.Http2, true, false, "2.0", "2.0", "HTTP/2")]

        // TLS http*: should always work
        [InlineData(HttpProtocols.Http1AndHttp2, true, false, null, "1.1", "HTTP/1.1")]
        [InlineData(HttpProtocols.Http1AndHttp2, true, false, "1.1", "1.1", "HTTP/1.1")]
        [InlineData(HttpProtocols.Http1AndHttp2, true, false, "2.0", "2.0", "HTTP/2")]
        public async Task UsesVersion(HttpProtocols protocols, bool tls, bool allowUnencryptedHttp2, string specified, string expectedVersion, string expectedResponse, bool failure = false)
        {
            bool oldMode = Http.AllowUnencryptedHttp2;
            try
            {
                Http.AllowUnencryptedHttp2 = allowUnencryptedHttp2;

                var uri = _server.GetUri(protocols, tls);
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

        public class Http2Server : IDisposable
        {
            private readonly IWebHost _host;

            private readonly int[] _ports = Enumerable.Range(10123, 6).ToArray();

            public string GetUri(HttpProtocols protocols, bool tls)
            {
                var index = protocols switch
                {
                    HttpProtocols.Http1 => tls ? 3 : 0,
                    HttpProtocols.Http2 => tls ? 4 : 1,
                    HttpProtocols.Http1AndHttp2 => tls ? 5 : 2,
                    _ => throw new ArgumentOutOfRangeException(nameof(protocols)),
                };
                return $"{(tls ? "https" : "http")}://localhost:{_ports[index]}/";
            }
            public Task WaitForShutdownAsync() => _host.WaitForShutdownAsync();

            public Http2Server()
            {
                _host = new WebHostBuilder()
                    .UseKestrel(options => {
                        options.ListenLocalhost(_ports[0], listenOptions =>
                        {
                            listenOptions.Protocols = HttpProtocols.Http1;
                        });
                        options.ListenLocalhost(_ports[1], listenOptions =>
                        {
                            listenOptions.Protocols = HttpProtocols.Http2;
                        });
                        options.ListenLocalhost(_ports[2], listenOptions =>
                        {
                            listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                        });
                        options.ListenLocalhost(_ports[3], listenOptions =>
                        {
                            listenOptions.Protocols = HttpProtocols.Http1;
                            listenOptions.UseHttps("certificate.pfx");
                        });
                        options.ListenLocalhost(_ports[4], listenOptions =>
                        {
                            listenOptions.Protocols = HttpProtocols.Http2;
                            listenOptions.UseHttps("certificate.pfx");
                        });
                        options.ListenLocalhost(_ports[5], listenOptions =>
                        {
                            listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                            listenOptions.UseHttps("certificate.pfx");
                        });
                    })
                    .Configure(app => {
                        app.Run(context => context.Response.WriteAsync(context.Request.Protocol));
                    })
                    .Build();
                _ = _host.RunAsync();
            }
            void IDisposable.Dispose()
                => _ = _host.StopAsync();
        }
    }
}
#endif
