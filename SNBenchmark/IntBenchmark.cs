using System;
using BenchmarkDotNet.Attributes;

namespace SNBenchmark
{
    [XmlExporterAttribute.Brief]
    [XmlExporter(fileNameSuffix: "xml", indentXml: true, excludeMeasurements: true)]
    public class IntBenchmark
    {
        readonly Generators generators = new Generators();
        Action<int[]> g;
        SortingNetworks.UnsafeSort<int> n;
        int[] d;

        [Params(4, 8, 12, 16, 32, 47, 64, 97, 128, 147, 256, 317, 512, 711, 1024, 1943, 2048, 3717, 4096)]
        public int Size { get; set; }

        //[Params("Asc", "Desc", "Rand")]
        [Params("Rand")]
        public string Pattern { get; set; }

        [GlobalSetup]
        public void GlobalSetup() {
            switch (Pattern) {
            case "Asc": g = generators.Ascending; break;
            case "Desc": g = generators.Descending; break;
            case "Rand": g = generators.FisherYates; break;
            default: throw new ArgumentOutOfRangeException(nameof(Pattern));
            }
            n = SortingNetworks.UnsafeSort.CreateInt(Size);
            d = new int[Size];
            Filler();
        }

        // Also used to simulate sorting.
        void Filler() {
            for (int i = 0; i < d.Length; ++i)
                d[i] = i;
        }

        void ArraySorter() => Array.Sort(d);
        
        unsafe void NetworkSorter() {
            fixed (int* p = d) n.Sorter(p, d.Length);
        }

        void Template(Action sorter, string what) {
            g(d);
            sorter();
            // Should leave the array sorted so no need to reinitialize it for the next iteration.
            int i;
            for (i = 0; i < d.Length && d[i] == i; ++i)
                ;   // no body
            if (i < d.Length)
                Environment.FailFast(what);
        }

        /// <summary>
        /// Baseline: Fill array with sorted numbers, overwrite with sorted sequence, and validate for being sorted.
        /// The first and last step are common for all benchmarks.
        /// </summary>
        [Benchmark(Baseline = true)]
        public void NoSort() => Template(Filler, "Unsorted [Baseline].");

        /// <summary>
        /// Sorting by using <c>Array.Sort()</c>.
        /// </summary>
        [Benchmark]
        public void ArraySort() => Template(ArraySorter, "Unsorted [ArraySort].");

        [Benchmark]
        public unsafe void NetworkSort() => Template(NetworkSorter, "Unsorted [NetworkSort].");
    }
}
