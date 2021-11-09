using System;
using BenchmarkDotNet.Attributes;

namespace SNBenchmark
{
    /// <summary>
    /// Benchmarks sorting on random data.
    /// </summary>
    public class RandomBenchmark : BenchmarkBase
    {
        readonly SortingNetworks.MWC1616Rand rng = new SortingNetworks.MWC1616Rand(new int[8] { 2, 3, 5, 7, 11, 13, 17, 19 });

        protected unsafe override void Generate(int[] data) {
            fixed (int* p = data) {
                for (int i = 0; i < data.Length / 4; ++i)
                    rng.Get4(p + 4 * i);
            }
        }

        [Benchmark(Baseline = true)]
        public new void Baseline() => base.Baseline();

        [Benchmark]
        public new void ArraySort() => base.ArraySort();

        [Benchmark]
        public new void NetworkSort() => base.NetworkSort();
    }
}
