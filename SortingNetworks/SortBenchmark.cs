using System;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;

namespace SortingNetworks
{
    public class SortBenchmark
    {
        readonly int[] data = new int[16];
        MWC1616Rand rng = new MWC1616Rand();
        Periodic16Expr sorter;

        [GlobalSetup]
        public void Initialize() {
            rng.Initialize(new int[8] { 3141, 592, 6535, 8979, 141, 173, 2236, 271828 });
            sorter = new Periodic16Expr();
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
            if (Validation.IsSorted(data))  // Negligible probability of happenning.
                throw new InvalidOperationException("Sorted.");
        }

        [Benchmark]
        public void ArraySort() {
            Setup();
            Array.Sort(data);
            if (!Validation.IsSorted(data))
                throw new InvalidOperationException("Unsorted.");
        }

        [Benchmark]
        public unsafe void NetworkSort() {
            Setup();
            fixed (int* p = data)
                Periodic16Ref.Sort(p);
            if (!Validation.IsSorted(data))
                throw new InvalidOperationException("Unsorted.");
        }

        // Omitted, compiled expressions have higher overhead than directly compiled code.
#if false
        [Benchmark]
        public unsafe void ExpressionSort() {
            Setup();
            fixed (int* p = data)
                sorter.Sort(p);
        }
#endif
    }
}
