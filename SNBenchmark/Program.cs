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
                    .WithOptions(ConfigOptions.StopOnFirstError | ConfigOptions.JoinSummary);

            var s = BenchmarkRunner.Run(typeof(Program).Assembly, config);
        }

        static unsafe void TestAESRand() {
            var r = new SortingNetworks.AESRand(new int[4] { 2, 3, 5, 7, });

            int[] idata = new int[4];
            float[] fdata = new float[4];

            for (int i = 0; i < 4; ++i) {
                fixed (int* p = idata)
                    r.Get4(p);
                fixed (float* p = fdata)
                    r.Get4U(p);
                fixed (float* p = fdata)
                    r.Get4N(p);
            }
        }

        static unsafe void TestMWC1616Rand() {
            var r = new SortingNetworks.MWC1616Rand(new int[8] { 2, 3, 5, 7, 11, 13, 17, 19, });

            int[] idata = new int[4];
            float[] fdata = new float[4];

            for (int i = 0; i < 4; ++i) {
                fixed (int* p = idata)
                    r.Get4(p);
                fixed (float* p = fdata)
                    r.Get4U(p);
                fixed (float* p = fdata)
                    r.Get4N(p);
            }
        }
    }
}
