using System;

namespace SNBenchmark
{
    /// <summary>
    /// Validation methods for verifying output of a sorting network.
    /// </summary>
    static class Validation
    {
        /// <summary>
        /// Validates <paramref name="sort"/> by exploiting theorem Z of TAOCOP section 5.3.4: it is
        /// sufficient to check that all 0-1 sequences (2^N of them) are sorted by the network.
        /// Only lengths of up to <c>28</c> are accepted.
        /// </summary>
        /// <param name="sort">An instance of sorting network to test.</param>
        /// <param name="size">Element count to test with.</param>
        /// <exception cref="ArgumentOutOfRangeException">Sorter's length is larger than 28.</exception>
        /// <exception cref="NotImplementedException">Validation has failed.</exception>
        public static unsafe void Check(SortingNetworks.UnsafeSort<int> sort, int size) {
            if (size < 4 || size > 32)
                throw new ArgumentOutOfRangeException(nameof(size), "Valid range is [4, 32].");
            
            var bits = new int[size];
            
            fixed (int* pbits = bits) {
                for (uint i = 0; i <= (1 << size) - 1; ++i) {
                    int popcnt = 0; // Number of ones in i
                    for (uint j = i, k = 0; k < size; ++k, j >>= 1) {
                        int b = (int)(j & 1);
                        pbits[k] = b;
                        popcnt += b;
                    }
                    
                    sort.Sorter(pbits, size);

                    for (int k = 0; k < size - popcnt; ++k)
                        if (pbits[k] != 0)
                            throw new NotImplementedException($"Result is not a permutation for bit pattern {i:X8}.");
                    
                    for (int k = size - popcnt; k < size; ++k)
                        if (pbits[k] != 1)
                            throw new NotImplementedException($"Result is not a permutation for bit pattern {i:X8}.");
                }
            }
        }

        /// <summary>
        /// Overload for float arrays; <see cref="Check(SortingNetworks.UnsafeSort{int}, int)"/>.
        /// </summary>
        public static unsafe void Check(SortingNetworks.UnsafeSort<float> sort, int size) {
            if (size < 4 || size > 32)
                throw new ArgumentOutOfRangeException(nameof(size), "Valid range is [4, 32].");

            var bits = new float[size];

            fixed (float* pbits = bits) {
                for (uint i = 0; i <= (1 << size) - 1; ++i) {
                    int popcnt = 0; // Number of ones in i
                    for (uint j = i, k = 0; k < size; ++k, j >>= 1) {
                        int b = (int)(j & 1);
                        pbits[k] = b;
                        popcnt += b;
                    }

                    sort.Sorter(pbits, size);

                    for (int k = 0; k < size - popcnt; ++k)
                        if (pbits[k] != 0)
                            throw new NotImplementedException($"Result is not a permutation for bit pattern {i:X8}.");

                    for (int k = size - popcnt; k < size; ++k)
                        if (pbits[k] != 1)
                            throw new NotImplementedException($"Result is not a permutation for bit pattern {i:X8}.");
                }
            }
        }

        /// <summary>
        /// Checks whether array <paramref name="data"/> is sorted.
        /// </summary>
        /// <returns>True if the input is sorted, false otherwise.</returns>
        public static bool IsSorted(int[] data) {
            for (int i = 1; i < data.Length; ++i)
                if (data[i] < data[i - 1])
                    return false;
            return true;
        }

        /// <summary>
        /// Checks whether array <paramref name="data"/> is sorted.
        /// </summary>
        /// <returns>True if the input is sorted, false otherwise.</returns>
        public static bool IsSorted(float[] data) {
            for (int i = 1; i < data.Length; ++i)
                if (data[i] < data[i - 1])
                    return false;
            return true;
        }
    }
}
