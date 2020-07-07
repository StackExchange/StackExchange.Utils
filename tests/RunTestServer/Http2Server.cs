using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace RunTestServer
{
    public class Http2Server : IDisposable
    {
        public static string Http1Uri => $"http://localhost:{Http1Port}";
        public static string Http2Uri => $"http://localhost:{Http2Port}";
        public static string Http1or2Uri => $"http://localhost:{Http1or2Port}";
        public static string Https1or2Uri => $"https://localhost:{Https1or2Port}";
        private readonly IWebHost _host;
        public static int Http1Port { get; } = 10123;
        public static int Http2Port { get; } = 10124;
        public static int Http1or2Port { get; } = 10125;
        public static int Https1or2Port { get; } = 10126;
        public Task WaitForShutdownAsync() => _host.WaitForShutdownAsync();

        public Http2Server()
        {
            _host = new WebHostBuilder()
                .UseKestrel(options => {
                    options.ListenLocalhost(Http1Port, listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http1;
                    });
                    options.ListenLocalhost(Http2Port, listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http2;
                    });
                    options.ListenLocalhost(Http1or2Port, listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                    });
                    options.ListenLocalhost(Https1or2Port, listenOptions =>
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
