using System;
using BenchmarkDotNet.Attributes;

namespace SNBenchmark
{
    [BenchmarkCategory("Invocation")]
    public class InvocationBenchmark
    {
        readonly int[] data;
        readonly SortingNetworks.MWC1616Rand rng = new SortingNetworks.MWC1616Rand(new int[8] { 2, 3, 5, 7, 11, 13, 17, 19 });
        readonly SortingNetworks.PeriodicInt direct;
        readonly SortingNetworks.UnsafeSort<int> @delegate;
        readonly SortingNetworks.Attic.Periodic16Expr expr;

        public unsafe InvocationBenchmark() {
            data = new int[16];
            direct = new SortingNetworks.PeriodicInt();
            @delegate = SortingNetworks.UnsafeSort<int>.CreateInt(data.Length);
            expr = new SortingNetworks.Attic.Periodic16Expr();
        }


        unsafe void Generate(int[] data) {
            fixed (int* p = data) {
                for (int i = 0; i < data.Length / 4; ++i)
                    rng.Get4(p + 4 * i);
            }
        }

        [Benchmark]
        public unsafe void DirectInvoke() {
            Generate(data);
            fixed (int* p = data)
                direct.Sort16(p);
        }

        [Benchmark]
        public unsafe void DelegateInvoke() {
            Generate(data);
            fixed (int* p = data)
                @delegate.Sorter(p);
        }

        [Benchmark]
        public unsafe void ExpressionInvoke() {
            Generate(data);
            fixed (int* p = data)
                expr.Sort(p);
        }
    }
}
