using System;
using System.Linq;

namespace SortingNetworks
{
    class Program
    {
        static void Main(string[] args) {
            var a = new int[] { 0, 1, 2, 3, 4, 5, 6 };
            var m = Builder.Swap();
            m(a, 2, 5);

            var output = string.Join(' ', a.Select(x => x.ToString()));
            Console.WriteLine(output);
        }
    }
}
