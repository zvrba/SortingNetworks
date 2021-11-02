using System;
using BenchmarkDotNet.Running;

namespace SortingNetworks
{
    class Program
    {
        static void Main(string[] args) {
            var summary = BenchmarkRunner.Run<SortBenchmark>();
        }

#if false
        static void Print(int[] a) {
            var s = string.Join(' ', a.Select(x => x.ToString("D2")));
            Console.WriteLine(s);
        }
#endif
    }
}
