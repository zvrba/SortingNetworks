using System;
using BenchmarkDotNet.Attributes;

namespace SNBenchmark
{
    /// <summary>
    /// Benchmarks sorting with reverse-sorted sequence.
    /// </summary>
    public class DecBenchmark : BenchmarkBase
    {
        protected override void Generate(int[] data) {
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
}
