using System;
using System.Collections.Generic;
using System.Text;

namespace SNBenchmark
{
    class Generators
    {
        readonly SortingNetworks.MWC1616Rand rng = new SortingNetworks.MWC1616Rand(new int[8] { 2, 3, 5, 7, 11, 13, 17, 19 });

        /// <summary>
        /// Fills data with integers from <c>0</c> to <c>data.Length-1</c> in ascending order.
        /// </summary>
        public void Ascending(int[] data) {
            for (int i = 0; i < data.Length; ++i)
                data[i] = i;
        }

        /// <summary>
        /// Fills data with integers from <c>0</c> to <c>data.Length-1</c> in descending order.
        /// </summary>
        public void Descending(int[] data) {
            for (int i = 0; i < data.Length; ++i)
                data[i] = data.Length - 1 - i;
        }

        /// <summary>
        /// Fills data with pseudo-random numbers.  Length of data must be a multiple of 4, otherwise the
        /// remaining elements will not be filled.
        /// </summary>
        /// <param name="data"></param>
        public unsafe void Random(int[] data) {
            fixed (int* p = data) {
                for (int i = 0; i < data.Length / 4; ++i)
                    rng.Get4(p + 4 * i);
            }
        }
    }
}
