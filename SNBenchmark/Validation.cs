﻿using System;

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
        /// <exception cref="ArgumentOutOfRangeException">Sorter's length is larger than 28.</exception>
        /// <exception cref="NotImplementedException">Validation has failed.</exception>
        public static unsafe void Check(SortingNetworks.UnsafeSort<int> sort) {
            if (sort.MaxLength > 28)
                throw new ArgumentOutOfRangeException($"The sorter's sequence length {sort.MaxLength} is too large.  Max acceptable value is 28.");
            
            var bits = new int[sort.MaxLength];
            var c = new int[2];

            fixed (int* b = bits) {
                for (int i = 0; i < 1 << sort.MaxLength; ++i) {
                    for (int j = i, k = 0; k < sort.MaxLength; ++k, j >>= 1) {
                        bits[k] = j & 1;
                        ++c[j & 1];
                    }
                    
                    sort.Sort(b);
                    
                    if (!IsSorted(bits))
                        throw new NotImplementedException($"Sorting failed for bit pattern {i:X8}.");

                    foreach (var bit in bits)
                        --c[bit];
                    if (c[0] != 0 || c[1] != 0)
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