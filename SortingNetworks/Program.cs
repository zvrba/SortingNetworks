using System;
using BenchmarkDotNet.Running;

namespace SortingNetworks
{
    class Program
    {
        static unsafe void Main(string[] args) {
            //Validation.Check(Periodic16Ref.Sort);

            //var es = new Periodic16Expr();
            //Validation.Check(es.Sort);

            var summary = BenchmarkRunner.Run<SortBenchmark>();
            //Periodic16Ref.Check();
        }

#if false
        static void Print(int[] a) {
            var s = string.Join(' ', a.Select(x => x.ToString("D2")));
            Console.WriteLine(s);
        }
#endif
    }
}
