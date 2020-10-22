using BenchmarkDotNet.Running;

namespace StackExchange.Utils.Benchmarks
{ 
    public class Program
    {
        public static void Main(string[] args) => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
