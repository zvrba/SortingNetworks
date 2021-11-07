using System;
using BenchmarkDotNet.Attributes;

namespace SortingNetworks
{
    /// <summary>
    /// Benchmarks sorting on random data.
    /// </summary>
    public class RandomBenchmark
    {
        readonly int[] data = new int[16];
        MWC1616Rand rng = new MWC1616Rand();
        Periodic16 periodic16;

        [GlobalSetup]
        public void Initialize() {
            rng.Initialize(new int[8] { 3141, 592, 6535, 8979, 141, 173, 2236, 271828 });
            periodic16 = new Periodic16();
        }

        unsafe void Setup() {
            fixed (int* p = data) {
                for (int i = 0; i < 4; ++i)
                    rng.Get4(p + 4 * i);
            }
            if (Validation.IsSorted(data))  // Negligible probability of happenning.
                throw new InvalidOperationException("Sorted.");
        }

        [Benchmark(Baseline = true)]
        public unsafe void Generate() {
            Setup();
        }

        [Benchmark]
        public void ArraySort() {
            Setup();
            Array.Sort(data);
            if (!Validation.IsSorted(data))
                throw new InvalidOperationException("Unsorted.");
        }

        [Benchmark]
        public unsafe void Periodic16Sort() {
            Setup();
            fixed (int* p = data)
                periodic16.Sort(p);
            if (!Validation.IsSorted(data))
                throw new InvalidOperationException("Unsorted.");
        }
    }
}
