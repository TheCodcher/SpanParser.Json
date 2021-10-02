using System;
using BenchmarkDotNet.Running;

namespace JsonSpanParserBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<JsonSpanParserLinqShortRun>();
            BenchmarkRunner.Run<NewtonsoftJsonLinqShortRun>();

            BenchmarkRunner.Run<JsonSpanParserPoolingRun>();
            BenchmarkRunner.Run<NewtonsoftJsonPoolingRun>();
        }
    }
}
