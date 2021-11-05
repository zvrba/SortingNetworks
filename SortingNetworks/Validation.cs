using System;
using System.Runtime.Intrinsics.X86;

namespace SortingNetworks
{
    /// <summary>
    /// Delegate type returned by compiling a network expression to lambda.
    /// </summary>
    public unsafe delegate void Sorter(int* data);

    /// <summary>
    /// Debugging and validation methods.
    /// </summary>
    static class Validation
    {
        /// <summary>
        /// Validates <paramref name="sort"/> by exploiting theorem Z of TAOCOP section 5.3.4: it is
        /// sufficient to check that all 0-1 sequences (2^16 of them) are sorted by the network.
        /// </summary>
        public static unsafe void Check(Sorter sort) {
            var bits = new int[16];
            fixed (int* b = bits) {
                for (int i = 0; i < 1 << 16; ++i) {
                    for (int j = i, k = 0; k < 16; ++k, j >>= 1)
                        bits[k] = j & 1;
                    sort(b);
                    if (!IsSorted(bits))
                        throw new InvalidOperationException($"Sorting failed for bit pattern {i:X4}.");
                }
            }
        }

        public static bool IsSorted(int[] data) {
            for (int i = 1; i < data.Length; ++i)
                if (data[i] < data[i - 1])
                    return false;
            return true;
        }
    }
}
