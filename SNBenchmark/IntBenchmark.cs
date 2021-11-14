using System;
using BenchmarkDotNet.Attributes;

namespace SNBenchmark
{
    public class IntBenchmark
    {
        readonly Generators generators = new Generators();
        
        Action<int[]> g;
        SortingNetworks.UnsafeSort<int> n;
        int[] d;

        [Params("Asc", "Desc", "Rand")]
        public string Pattern { get; set; }

        [Params(4, 8, 16)]
        public int Size { get; set; }

        [GlobalSetup]
        public void GlobalSetup() {
            switch (Pattern) {
            case "Asc": g = generators.Ascending; break;
            case "Desc": g = generators.Descending; break;
            case "Rand": g = generators.Random; break;
            default: throw new ArgumentOutOfRangeException(nameof(Pattern));
            }
            n = SortingNetworks.UnsafeSort.CreateInt(Size);
            d = new int[Size];
        }

        /// <summary>
        /// Baseline: Fill array with sorted numbers, overwrite with sorted sequence, and validate for being sorted.
        /// The first and last step are common for all benchmarks.
        /// </summary>
        [Benchmark(Baseline = true)]
        public void NoSort() {
            g(d);
            for (int i = 0; i < d.Length; ++i)   // Simulate sorting.
                d[i] = i;
            if (!Validation.IsSorted(d))
                Environment.FailFast("Unsorted [Baseline].");
        }

        /// <summary>
        /// Sorting by using <c>Array.Sort()</c>.
        /// </summary>
        [Benchmark]
        public void ArraySort() {
            g(d);
            Array.Sort(d);
            if (!Validation.IsSorted(d))
                Environment.FailFast("Unsorted [ArraySort].");
        }

        [Benchmark]
        public unsafe void NetworkSort() {
            g(d);
            fixed (int* p = d)
                n.Sort(p);
            if (!Validation.IsSorted(d))
                Environment.FailFast("Unsorted [NetworkSort].");
        }
    }
}
