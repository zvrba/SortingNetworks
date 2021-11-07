using System;
using System.Collections.Generic;
using System.Text;

namespace SNBenchmark
{
    /// <summary>
    /// Common methods for sorting benchmarks.  This is an abstract class as BenchmarkDotNet can't parametrize
    /// benchmarks with lambdas and "runs" only types, not instances.
    /// </summary>
    public abstract class BenchmarkBase
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
}
