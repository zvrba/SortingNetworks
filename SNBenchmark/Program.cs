using System;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace SNBenchmark
{
    unsafe class Program
    {
        static void Main(string[] args) {
            if (args.Length == 0)
                Usage();

            if (args[0] == "VI") {
                ValidateInt();
            }
            else if (args[0] == "VF") {
                ValidateFloat();
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
            Console.WriteLine("USAGE: {VI | VF | B} [argument...]");
            Console.WriteLine("VI validates int sorting networks for all sizes up to 32.");
            Console.WriteLine("VF validates float sorting networks for all sizes up to 32.");
            Console.WriteLine("B runs benchmarks with arguments following it.");
            Environment.Exit(0);
        }

        static void ValidateInt() {
            for (int size = 4; size <= 32; ++size) {
                var n = SortingNetworks.UnsafeSort<int>.Create(size);
                Console.Write($"Validating size {size:D2}: ");
                try {
                    Validation.Check(n, size);
                    Console.WriteLine("OK");
                }
                catch (NotImplementedException e) {
                    Console.WriteLine($"FAILED: {e.Message}");
                }
            }
        }

        static void ValidateFloat() {
            for (int size = 4; size <= 32; ++size) {
                var n = SortingNetworks.UnsafeSort<float>.Create(size);
                Console.Write($"Validating size {size:D2}: ");
                try {
                    Validation.Check(n, size);
                    Console.WriteLine("OK");
                }
                catch (NotImplementedException e) {
                    Console.WriteLine($"FAILED: {e.Message}");
                }
            }
        }

        // This exists only for sporadic testing and debugging.
        static void Test() {
            var d = new int[11157];
            int[] dc;
            var g = new Generators();
            var nn = SortingNetworks.UnsafeSort<int>.Create(d.Length);
            for (int i = 0; i < d.Length; ++i) d[i] = i;

            var iteration = 0;
            while (true) {
                ++iteration;
                //if ((iteration % 1000) == 0)
                //    Console.WriteLine(iteration);
                g.FisherYates(d);
                dc = (int[])d.Clone();
                fixed (int* p = d)
                    nn.Sorter(p, d.Length);
                for (int i = 0; i < d.Length; ++i)
                    if (d[i] != i)
                        throw new NotImplementedException();
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
