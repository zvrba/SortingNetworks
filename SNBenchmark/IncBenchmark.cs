using System;
using BenchmarkDotNet.Attributes;

namespace SNBenchmark
{
    /// <summary>
    /// Benchmarks sorting with already sorted sequence.
    /// </summary>
    public class IncBenchmark : BenchmarkBase
    {
        protected override void Generate(int[] data) {
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
}
