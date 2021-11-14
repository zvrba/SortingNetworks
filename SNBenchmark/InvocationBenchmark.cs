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
        readonly Sorter sorter = new Sorter();
        readonly ISorter isorter = new Sorter();
        readonly Action<int[]> dsorter = (new Sorter()).Sort;

        // Generate a sorted array to remove all data-dependent variability.
        [GlobalSetup]
        public void GlobalSetup() {
            for (int i = 0; i < data.Length; ++i) data[i] = i;
        }

        [Benchmark]
        public void DirectInvoke() {
            sorter.Sort(data);
        }

        [Benchmark]
        public void InterfaceInvoke() {
            isorter.Sort(data);
        }

        [Benchmark]
        public unsafe void DelegateInvoke() {
            dsorter(data);
        }

        interface ISorter
        {
            void Sort(int[] data);
        }

        class Sorter : ISorter
        {
            public unsafe void Sort(int[] data) {
                Array.Sort(data);
            }
        }
    }
}
