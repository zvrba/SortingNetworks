using System;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace SNBenchmark
{
    unsafe class Program
    {
        static void Main(string[] args) {
#if false
            TestIntBig();
            Environment.Exit(0);
#endif

            if (args.Length == 0)
                Usage();

            if (args[0] == "V") {
                Validate();
            }
            else if (args[0] == "B") {
                //var ss = new BenchmarkDotNet.Reports.BenchmarkReport
                // TODO: SummaryStyle; InvariantCulture
                var config = ManualConfig.Create(DefaultConfig.Instance)
                        .WithOptions(ConfigOptions.StopOnFirstError | ConfigOptions.JoinSummary);
                var args1 = new string[args.Length - 1];
                Array.Copy(args, 1, args1, 0, args.Length - 1);
                BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args1);
            }
            else {
                Usage();
            }

            Environment.Exit(0);
        }

        static void Usage() {
            Console.WriteLine("USAGE: {V | B} [argument...]");
            Console.WriteLine("V validates sorting methods for all sizes up to 28.");
            Console.WriteLine("B runs benchmarks with arguments following it.");
            Environment.Exit(0);
        }

        static void TestIntBig() {
            var g = new Generators();
            var d = new int[1024];
            var n = SortingNetworks.UnsafeSort.CreateInt(d.Length);
            for (int i = 0; i < d.Length; ++i) d[i] = i;
            g.FisherYates(d);
            fixed (int* p = d)
                n.Sort(p);
            for (int i = 0; i < d.Length; ++i)
                if (d[i] != i)
                    throw new InvalidOperationException("Unsorted.");
        }

        static unsafe void Validate() {
            for (int size = 4; size <= 16; size *= 2) {
                var d = new int[size];
                var n = SortingNetworks.UnsafeSort.CreateInt(size);
                Validate(n);
            }
        }

        static void Validate(SortingNetworks.UnsafeSort<int> sorter) {
            Console.Write($"Validating size {sorter.MaxLength:D2}: ");
            try {
                Validation.Check(sorter);
                Console.WriteLine("OK");
            } catch (Exception e) {
                Console.WriteLine($"FAILED: {e.Message}");
            }
        }

        static unsafe void TestAESRand() {
            var r = new SortingNetworks.AESRand(new int[4] { 2, 3, 5, 7, });

            int[] idata = new int[4];
            float[] fdata = new float[4];

            for (int i = 0; i < 4; ++i) {
                fixed (int* p = idata)
                    r.Get4(p);
                fixed (float* p = fdata)
                    r.Get4U(p);
                fixed (float* p = fdata)
                    r.Get4N(p);
            }
        }

        static unsafe void TestMWC1616Rand() {
            var r = new SortingNetworks.MWC1616Rand(new int[8] { 2, 3, 5, 7, 11, 13, 17, 19, });

            int[] idata = new int[4];
            float[] fdata = new float[4];

            for (int i = 0; i < 4; ++i) {
                fixed (int* p = idata)
                    r.Get4(p);
                fixed (float* p = fdata)
                    r.Get4U(p);
                fixed (float* p = fdata)
                    r.Get4N(p);
            }
        }
    }
}
