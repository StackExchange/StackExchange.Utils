using BenchmarkDotNet.Running;
using Benchmarks.Benchmarks;

namespace Benchmarks
{ 
    public class Program
    {
        public static void Main(string[] args)
        {
             BenchmarkRunner.Run<ResponseBuffering>();
        }
    }
}
