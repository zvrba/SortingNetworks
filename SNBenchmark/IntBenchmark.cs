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
        readonly int[] data;
        readonly SortingNetworks.PeriodicInt periodicInt = new SortingNetworks.PeriodicInt();


        /// <summary>
        /// This method is expected to fill in <paramref name="data"/> with the pattern to be sorted.
        /// </summary>
        protected abstract void Generate(int[] data);

        protected IntBenchmarkBase(int length) {
            data = new int[length];
        }

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
        /// Sorting of 16 elements by using the optimized periodic sorting network.
        /// </summary>
        protected unsafe void NetworkSort16() {
            Generate(data);
            fixed (int* p = data)
                periodicInt.Sort16(p);
            if (!SortingNetworks.Validation.IsSorted(data))
                Environment.FailFast("Unsorted [NetworkSort].");
        }

        protected unsafe void NetworkSort32() {
            Generate(data);
            fixed (int* p = data)
                periodicInt.Sort32(p);
            if (!SortingNetworks.Validation.IsSorted(data))
                Environment.FailFast("Unsorted [NetworkSort].");
        }
    }

    /// <summary>
    /// Benchmarks sorting with already sorted sequence.
    /// </summary>
    public class IntIncBenchmark16 : IntBenchmarkBase
    {
        public IntIncBenchmark16() : base(16) { }

        protected sealed override void Generate(int[] data) {
            for (int i = 0; i < data.Length; ++i)
                data[i] = i;
        }

        [Benchmark(Baseline = true)]
        public new void Baseline() => base.Baseline();

        [Benchmark]
        public new void ArraySort() => base.ArraySort();

        [Benchmark]
        public void NetworkSort() => base.NetworkSort16();
    }

    /// <summary>
    /// Benchmarks sorting with reverse-sorted sequence.
    /// </summary>
    public class IntDecBenchmark16 : IntBenchmarkBase
    {
        public IntDecBenchmark16() : base(16) { }

        protected sealed override void Generate(int[] data) {
            for (int i = 0; i < data.Length; ++i)
                data[i] = data.Length - i - 1;
        }

        [Benchmark(Baseline = true)]
        public new void Baseline() => base.Baseline();

        [Benchmark]
        public new void ArraySort() => base.ArraySort();

        [Benchmark]
        public void NetworkSort() => base.NetworkSort16();
    }

    /// <summary>
    /// Benchmarks sorting on random data.
    /// </summary>
    public class IntRandBenchmark16 : IntBenchmarkBase
    {
        readonly SortingNetworks.MWC1616Rand rng = new SortingNetworks.MWC1616Rand(new int[8] { 2, 3, 5, 7, 11, 13, 17, 19 });

        public IntRandBenchmark16() : base(16) { }

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
        public void NetworkSort() => base.NetworkSort16();
    }

    /// <summary>
    /// Benchmarks sorting on random data.
    /// </summary>
    public class IntRandBenchmark32 : IntBenchmarkBase
    {
        readonly SortingNetworks.MWC1616Rand rng = new SortingNetworks.MWC1616Rand(new int[8] { 2, 3, 5, 7, 11, 13, 17, 19 });

        public IntRandBenchmark32() : base(32) { }

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
        public void NetworkSort() => base.NetworkSort32();
    }
}
