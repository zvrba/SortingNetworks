using BenchmarkDotNet.Attributes;

using System;

namespace SNBenchmark
{
    /// <summary>
    /// <c>Array.Sort</c> uses an introsort algorithm that has <c>O(n*log(n))</c> complexity.  This benchmark
    /// generates data for estimating the constant hidden in the O-term.  Only random pattern is used.
    /// </summary>
    public class ArraySortConstantEstimation
    {
        readonly Generators generators = new Generators();
        int[] d;

        [Params(32, 64, 128, 256, 1024, 2048, 4096, 8192, 16384)]
        public int Size { get; set; }

        [GlobalSetup]
        public void GlobalSetup() {
            d = new int[Size];
        }

        [Benchmark(Baseline = true)]
        public void NoSort() {
            generators.Random(d);
        }

        [Benchmark]
        public void ArraySort() {
            generators.Random(d);
            Array.Sort(d);
        }
    }
}
