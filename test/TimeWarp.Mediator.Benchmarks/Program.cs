// Modified by Steven T. Cramer
using BenchmarkDotNet.Running;

namespace TimeWarp.Mediator.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args) => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}