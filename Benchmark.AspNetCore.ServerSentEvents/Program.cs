using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Benchmark.AspNetCore.ServerSentEvents.Benchmarks;

namespace Benchmark.AspNetCore.ServerSentEvents
{
    class Program
    {
        static void Main(string[] args)
        {
            Summary serverSentEventsServiceSummary = BenchmarkRunner.Run<ServerSentEventsServiceBenchmarks>();

            System.Console.ReadKey();
        }
    }
}
