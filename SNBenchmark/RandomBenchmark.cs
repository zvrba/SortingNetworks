using System;
using BenchmarkDotNet.Attributes;

namespace SNBenchmark
{
    /// <summary>
    /// Benchmarks sorting on random data.
    /// </summary>
    public class RandomBenchmark : BenchmarkBase
    {
        // NB! Cannot be readonly.
        SortingNetworks.MWC1616Rand rng = new SortingNetworks.MWC1616Rand();

        protected unsafe override void Generate(int[] data) {
            fixed (int* p = data) {
                for (int i = 0; i < data.Length / 4; ++i)
                    rng.Get4(p + 4 * i);
            }
        }

        [GlobalSetup]
        public void Initialize() {
            rng.Initialize(new int[8] { 3141, 592, 6535, 8979, 141, 173, 2236, 271828 });
        }

        [Benchmark(Baseline = true)]
        public new void Baseline() => base.Baseline();

        [Benchmark]
        public new void ArraySort() => base.ArraySort();

        [Benchmark]
        public new void NetworkSort() => base.NetworkSort();
    }
}
