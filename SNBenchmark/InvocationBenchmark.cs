using System;
using BenchmarkDotNet.Attributes;

namespace SNBenchmark
{
    [BenchmarkCategory("Invocation")]
    public class ExpressionInvocationBenchmark
    {
        readonly int[] data = new int[16];
        readonly SortingNetworks.Attic.Periodic16Expr expr = new SortingNetworks.Attic.Periodic16Expr();

        // Sets up data array to be sorted so as to have minimum possible data-dependent variation.
        [GlobalSetup]
        public void GlobalSetup() {
            for (int i = 0; i < data.Length; ++i) data[i] = i;
        }

        [Benchmark]
        public unsafe void DirectInvoke() {
            fixed (int* p = data)
                SortingNetworks.Attic.Periodic16Branchless.Sort(p);
        }

        [Benchmark]
        public unsafe void ExpressionInvoke() {
            fixed (int* p = data)
                expr.Sort(p);
        }
    }

    [BenchmarkCategory("Invocation")]
    public class InvocationBenchmark
    {
        readonly int[] data = new int[16];
        readonly SortingNetworks.UnsafeSort<int> asorter = SortingNetworks.UnsafeSort.CreateInt(16);
        readonly SortingNetworks.PeriodicInt csorter = new SortingNetworks.PeriodicInt();

        [GlobalSetup]
        public void GlobalSetup() {
            for (int i = 0; i < data.Length; ++i) data[i] = i;
        }

        [Benchmark]
        public unsafe void AbstractInvoke() {
            fixed (int* p = data)
                asorter.Sort(p);
        }

        [Benchmark]
        public unsafe void ConcreteInvoke() {
            fixed (int* p = data)
                csorter.Sort16(p);
        }
    }
}
