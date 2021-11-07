using System;
using BenchmarkDotNet.Running;

namespace SortingNetworks
{
    class Program
    {
        static unsafe void Main(string[] args) {
            var periodic16 = new Periodic16();
            Validation.Check(periodic16.Sort);

            //var es = new Periodic16Expr();
            //Validation.Check(es.Sort);

#if false
            MWC1616Rand rand = new MWC1616Rand();
            rand.Initialize(new int[8] { 3141, 592, 6535, 8979, 141, 173, 2236, 271828 });

            var data = new int[4];

            for (int i = 0; i < 16; ++i) {
                fixed (int* p = data)
                    rand.Get4(p);
                Console.WriteLine("{0:X8} {1:X8} {2:X8} {3:X8}", data[0], data[1], data[2], data[3]);
            }
#endif

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
