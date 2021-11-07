using System;
using BenchmarkDotNet.Attributes;

namespace SortingNetworks
{
    /// <summary>
    /// Benchmarks sorting of already sorted data.
    /// </summary>
    public class IncBenchmark
    {
        readonly int[] data = new int[16];
        Periodic16 periodic16;

        [GlobalSetup]
        public void Initialize() {
            periodic16 = new Periodic16();
        }

        void Setup() {
            for (int i = 0; i < 16; ++i)
                data[i] = i;
            if (!Validation.IsSorted(data))  // Never happens, but needed for the baseline.
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
