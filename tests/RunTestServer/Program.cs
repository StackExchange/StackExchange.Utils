using System;
using System.Threading.Tasks;

namespace RunTestServer
{
    internal static class Program
    {
        private static async Task Main()
        {
            Console.WriteLine("Creating server...");
            using var server = new Http2Server();
            await server.WaitForShutdownAsync();
        }
    }
}
