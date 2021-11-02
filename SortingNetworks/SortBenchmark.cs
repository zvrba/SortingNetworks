using System;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;

namespace SortingNetworks
{
    public class SortBenchmark
    {
        readonly int[] data = new int[16];
        readonly Random rng = new Random();

        void Setup() {
            for (int i = 0; i < 16; ++i) data[i] = rng.Next(256);
        }

        [Benchmark(Baseline = true)]
        public unsafe void Generate() {
            Setup();
        }

        [Benchmark]
        public void ArraySort() {
            Generate();
            Array.Sort(data);
        }

        [Benchmark]
        public unsafe void NetworkSort() {
            Generate();
            fixed (int* p = data)
                Periodic16Ref.Sort(p);
        }
    }
}
