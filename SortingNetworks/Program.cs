using System;
using System.Linq;

namespace SortingNetworks
{
    class Program
    {
        static void Main(string[] args) {
            Periodic16Ref.Test();
        }

        static void Print(int[] a) {
            var s = string.Join(' ', a.Select(x => x.ToString("D2")));
            Console.WriteLine(s);
        }
    }
}
