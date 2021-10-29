using System;
using System.Collections.Generic;
using System.Text;

namespace SortingNetworks
{
    /// <summary>
    /// Perfect shuffle permutation on 2^N elements.
    /// </summary>
    class PerfectShuffle
    {
        /// <summary>
        /// Represents a permutation that maps index <c>i</c> to <c>Permutation[i]</c>.
        /// </summary>
        public readonly int[] Permutation;

        private readonly int topbit;

        public PerfectShuffle(int n) {
            if (n < 2)
                throw new ArgumentOutOfRangeException(nameof(n), "Element count must be at least 2.");

            // Simple bit count.
            int c = 0;
            for (int i = n; i > 0; i >>= 1)
                c += i & 1;

            if (c != 1)
                throw new ArgumentOutOfRangeException(nameof(n), "Element count must be a power of 2.");

            Permutation = new int[n];
            topbit = n >> 1;
            Reset();
        }

        /// <summary>
        /// Resets <see cref="Permutation"/> to the identity permutation.
        /// </summary>
        public void Reset() {
            for (int i = 0; i < Permutation.Length; ++i) Permutation[i] = i;
        }

        /// <summary>
        /// Applies perfect shuffle to current  <see cref="Permutation"/>.
        /// </summary>
        public void Shuffle() {
            for (int i = 0; i < Permutation.Length / 2; ++i)
                Swap(ref Permutation[i], ref Permutation[i | topbit]);

            static void Swap(ref int a, ref int b) => (a, b) = (b, a);
        }
    }
}
