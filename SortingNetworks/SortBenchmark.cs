using System;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;

namespace SortingNetworks
{
    public class SortBenchmark
    {
        readonly int[] data = new int[16];
        readonly AESRand rng = new AESRand();
        Periodic16Expr sorter;

        [GlobalSetup]
        public void CreateNetwork() {
            sorter = new Periodic16Expr();
            rng.Initialize(1);
        }

        unsafe void Setup() {
            fixed (int* p = data) {
                for (int i = 0; i < 4; ++i)
                    rng.Get4(p + 4 * i);
            }
        }

        [Benchmark(Baseline = true)]
        public unsafe void Generate() {
            Setup();
        }

        [Benchmark]
        public void ArraySort() {
            Setup();
            Array.Sort(data);
        }

        [Benchmark]
        public unsafe void NetworkSort() {
            Setup();
            fixed (int* p = data)
                Periodic16Ref.Sort(p);
        }

        [Benchmark]
        public unsafe void ExpressionSort() {
            Setup();
            fixed (int* p = data)
                sorter.Sort(p);
        }
    }
}
