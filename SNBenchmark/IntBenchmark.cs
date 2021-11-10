using System;
using BenchmarkDotNet.Attributes;

namespace SNBenchmark
{
    /// <summary>
    /// Common methods for sorting benchmarks.  This is an abstract class as BenchmarkDotNet can't parametrize
    /// benchmarks with lambdas and "runs" only types, not instances.
    /// </summary>
    public abstract class IntBenchmarkBase
    {
        readonly int[] data = new int[16];
        readonly SortingNetworks.Periodic16 periodic16 = new SortingNetworks.Periodic16();

        /// <summary>
        /// This method is expected to fill in <paramref name="data"/> with the pattern to be sorted.
        /// </summary>
        protected abstract void Generate(int[] data);

        // The following methods are intended to be called from public benchmark methods.

        /// <summary>
        /// Baseline: Fill array with sorted numbers, overwrite with sorted sequence, and validate for being sorted.
        /// The first and last step are common for all benchmarks.
        /// </summary>
        protected void Baseline() {
            Generate(data);
            for (int i = 0; i < data.Length; ++i)   // Simulate sorting.
                data[i] = i;
            if (!SortingNetworks.Validation.IsSorted(data))
                Environment.FailFast("Unsorted [Baseline].");
        }

        /// <summary>
        /// Sorting by using <c>Array.Sort()</c>.
        /// </summary>
        protected void ArraySort() {
            Generate(data);
            Array.Sort(data);
            if (!SortingNetworks.Validation.IsSorted(data))
                Environment.FailFast("Unsorted [ArraySort].");
        }

        /// <summary>
        /// Sorting by using the optimized periodic sorting network.
        /// </summary>
        protected unsafe void NetworkSort() {
            Generate(data);
            fixed (int* p = data)
                periodic16.Sort(p);
            if (!SortingNetworks.Validation.IsSorted(data))
                Environment.FailFast("Unsorted [NetworkSort].");
        }
    }

    /// <summary>
    /// Benchmarks sorting with already sorted sequence.
    /// </summary>
    public class IntIncBenchmark : IntBenchmarkBase
    {
        protected sealed override void Generate(int[] data) {
            for (int i = 0; i < data.Length; ++i)
                data[i] = i;
        }

        [Benchmark(Baseline = true)]
        public new void Baseline() => base.Baseline();

        [Benchmark]
        public new void ArraySort() => base.ArraySort();

        [Benchmark]
        public new void NetworkSort() => base.NetworkSort();
    }

    /// <summary>
    /// Benchmarks sorting with reverse-sorted sequence.
    /// </summary>
    public class IntDecBenchmark : IntBenchmarkBase
    {
        protected sealed override void Generate(int[] data) {
            for (int i = 0; i < data.Length; ++i)
                data[i] = data.Length - i - 1;
        }

        [Benchmark(Baseline = true)]
        public new void Baseline() => base.Baseline();

        [Benchmark]
        public new void ArraySort() => base.ArraySort();

        [Benchmark]
        public new void NetworkSort() => base.NetworkSort();
    }

    /// <summary>
    /// Benchmarks sorting on random data.
    /// </summary>
    public class IntRandBenchmark : IntBenchmarkBase
    {
        readonly SortingNetworks.MWC1616Rand rng = new SortingNetworks.MWC1616Rand(new int[8] { 2, 3, 5, 7, 11, 13, 17, 19 });

        protected sealed unsafe override void Generate(int[] data) {
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
