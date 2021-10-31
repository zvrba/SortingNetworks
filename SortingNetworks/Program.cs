using System;
using System.Linq;

namespace SortingNetworks
{
    class Program
    {
        static void Main(string[] args) {
            var p = new PerfectShuffle(8);
            
            p.Shuffle();
            Print(p.Permutation);

            p.Shuffle();
            Print(p.Permutation);
        }

        static void Print(int[] a) {
            var s = string.Join(' ', a.Select(x => x.ToString("D2")));
            Console.WriteLine(s);
        }
    }
}
