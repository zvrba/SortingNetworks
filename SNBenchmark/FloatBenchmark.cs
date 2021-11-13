using System;
using BenchmarkDotNet.Attributes;

namespace SNBenchmark
{
#if false
    /// <summary>
    /// This is copy-paste of <see cref="IntBenchmarkBase"/> because it's next to impossible to write generic code over arrays.
    /// </summary>
    public abstract class FloatBenchmarkBase
    {
        readonly float[] data = new float[16];
        readonly SortingNetworks.Periodic16 periodic16 = new SortingNetworks.Periodic16();

        protected abstract void Generate(float[] data);

        protected void Baseline() {
            Generate(data);
            for (int i = 0; i < data.Length; ++i)   // Simulate sorting.
                data[i] = i;
            if (!SortingNetworks.Validation.IsSorted(data))
                Environment.FailFast("Unsorted [Baseline].");
        }

        protected void ArraySort() {
            Generate(data);
            Array.Sort(data);
            if (!SortingNetworks.Validation.IsSorted(data))
                Environment.FailFast("Unsorted [ArraySort].");
        }

        /// <summary>
        /// Exploits the property of float representation where floats can be sorted by using integer comparisons.
        /// This does not account for NaNs.
        /// </summary>
        protected unsafe void NetworkSort() {
            Generate(data);
            fixed (float* p = data)
                periodic16.Sort((int*)p);
            if (!SortingNetworks.Validation.IsSorted(data))
                Environment.FailFast("Unsorted [NetworkSort].");
        }
    }


    /// <summary>
    /// Benchmarks sorting floating point numbers in any range.
    /// </summary>
    public class FloatRandUBenchmark : FloatBenchmarkBase
    {
        readonly SortingNetworks.MWC1616Rand rng = new SortingNetworks.MWC1616Rand(new int[8] { 2, 3, 5, 7, 11, 13, 17, 19 });

        protected sealed unsafe override void Generate(float[] data) {
            fixed (float* p = data) {
                for (int i = 0; i < data.Length / 4; ++i)
                    rng.Get4U(p + 4 * i);
            }
        }

        [Benchmark(Baseline = true)]
        public new void Baseline() => base.Baseline();

        [Benchmark]
        public new void ArraySort() => base.ArraySort();

        [Benchmark]
        public new void NetworkSort() => base.NetworkSort();
    }

    /// <summary>
    /// Benchmarks sorting floating point numbers in any range.
    /// </summary>
    public class FloatRandNBenchmark : FloatBenchmarkBase
    {
        readonly SortingNetworks.MWC1616Rand rng = new SortingNetworks.MWC1616Rand(new int[8] { 2, 3, 5, 7, 11, 13, 17, 19 });

        protected sealed unsafe override void Generate(float[] data) {
            fixed (float* p = data) {
                for (int i = 0; i < data.Length / 4; ++i)
                    rng.Get4N(p + 4 * i);
            }
        }

        [Benchmark(Baseline = true)]
        public new void Baseline() => base.Baseline();

        [Benchmark]
        public new void ArraySort() => base.ArraySort();

        [Benchmark]
        public new void NetworkSort() => base.NetworkSort();
    }

#endif
}
