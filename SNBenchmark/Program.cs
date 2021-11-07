using System;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace SNBenchmark
{
    unsafe class Program
    {
        static void Main(string[] args) {
            // Exhaustive validation of the 16-element block.
            var periodic16 = new SortingNetworks.Periodic16();
            SortingNetworks.Validation.Check(periodic16.Sort);

            var config = ManualConfig.Create(DefaultConfig.Instance)
                    .WithOptions(ConfigOptions.StopOnFirstError);

            var s = BenchmarkRunner.Run(typeof(Program).Assembly, config);
        }
    }
}
