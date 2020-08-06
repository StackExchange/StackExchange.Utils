using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using StackExchange.Utils;

namespace Benchmarks.Benchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net472)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    public class ResponseBuffering
    {
        private static string _jsonUrl = "https://jsonplaceholder.typicode.com/photos";

        public class SamplePhoto
        {
            public int albumId { get; set; }
            public int id { get; set; }
            public string title { get; set; }
            public string url { get; set; }
            public string thumbnailUrl { get; set; }
        }


        [Benchmark(Description = "ExpectHttpSuccess with buffering")]
        public async Task<bool> ExpectHttpSuccessWithBuffering() => (await Http.Request(_jsonUrl).ExpectHttpSuccess().GetAsync()).Success;

        [Benchmark(Description = "ExpectHttpSuccess without buffering")]
        public async Task<bool> ExpectHttpSuccessWithOutBuffering() => (await Http.Request(_jsonUrl).WithoutResponseBuffering().ExpectHttpSuccess().GetAsync()).Success;



        [Benchmark(Description = "ExpectJson with buffering")]
        public async Task<List<SamplePhoto>> ExpectJsonWithBuffering() => (await Http.Request(_jsonUrl).ExpectJson<List<SamplePhoto>>().GetAsync()).Data;

        [Benchmark(Description = "ExpectJson without buffering")]
        public async Task<List<SamplePhoto>> ExpectJsonWithoutBuffering() => (await Http.Request(_jsonUrl).WithoutResponseBuffering().ExpectJson<List<SamplePhoto>>().GetAsync()).Data;


        [Benchmark(Description = "ExpectString with buffering")]
        public async Task<bool> ExpectStringWithBuffering() => (await Http.Request(_jsonUrl).ExpectString().GetAsync()).Success;

        [Benchmark(Description = "ExpectString without buffering")]
        public async Task<string> ExpectStringWithOutBuffering() => (await Http.Request(_jsonUrl).WithoutResponseBuffering().ExpectString().GetAsync()).Data;


        [Benchmark(Description = "ExpectByteArray with buffering")]
        public async Task<bool> ExpectByteArrayWithBuffering() => (await Http.Request(_jsonUrl).ExpectByteArray().GetAsync()).Success;

        [Benchmark(Description = "ExpectByteArray without buffering")]
        public async Task<byte[]> ExpectByteArrayWithOutBuffering() => (await Http.Request(_jsonUrl).WithoutResponseBuffering().ExpectByteArray().GetAsync()).Data;

    }
}

