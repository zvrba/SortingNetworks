using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

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

        /// <summary>
        /// Rearranges the existing contents of <paramref name="data"/> according to a random permutation.
        /// </summary>
        public unsafe void FisherYates(int[] data) {
            var r = stackalloc uint[4]; // Randomness
            int k = 4;                  // Randomness is initially used up. j is temp.
            int j;
            Vector128<uint> ar;
            
            // Use pointer throughout to avoid bound checks.
            // Also, we're jumping around the array so the direction of the iteration doesn't matter.
            fixed (int* p = data) {
                for (int i = data.Length - 1; i > 0; --i) {
                    // Generate randomness if empty.
                    if (k == 4) {
                        ar = rng.Get4().AsUInt32();
                        Sse2.Store(r, ar);
                        k = 0;
                    }
                    j = (int)(r[k++] % (i + 1));    // Random int between [0, i]
                    (p[i], p[j]) = (p[j], p[i]);    // Exchange.
                }
            }
        }
    }
}
