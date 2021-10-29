using System;
using System.Linq;

namespace SortingNetworks
{
    class Program
    {
        static void Main(string[] args) {
            var p = new PerfectShuffle(8);
            p.Shuffle();
            var output = string.Join(' ', p.Permutation.Select(x => x.ToString()));
            Console.WriteLine(output);
        }
    }
}
