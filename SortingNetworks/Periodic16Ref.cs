using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace SortingNetworks
{
    using V = System.Runtime.Intrinsics.Vector256<int>;

    /// <summary>
    /// Reference implementation of 16-element periodic sorting network.
    /// </summary>
    static class Periodic16Ref
    {
        // All zeros
        static readonly V Zero = Vector256.Create(0);
        // All ones
        static readonly V Complement = Avx2.CompareEqual(Zero, Zero);
        // FF00FF00 (1 digit = 32 bits)
        static readonly V AlternatingMaskHi64 = Avx2.Xor(Complement, Avx2.ShiftRightLogical128BitLane(Complement, 8));

        const int ShufRev4 = 0x1B;

        /// <summary>
        /// Sorts 16 elements starting at <paramref name="data"/>.
        /// </summary>
        /// <param name="data"></param>
        public static unsafe void Sort(int* data) {
            V tmp1, tmp2, tmp3;

            // lo, hi are intermediate results after each stage and input to next one.
            var lo = Avx.LoadVector256(data);
            var hi = Avx.LoadVector256(data + 8);


            // STAGE 1:
            // 76543210
            // 89ABCDEF

            tmp1 = Avx2.Shuffle(hi, ShufRev4);          // CDEF89AB
            hi = Avx2.Permute2x128(tmp1, tmp1, 1);      // 89ABCDEF
            CompareSwap(ref lo, ref hi);

            // STAGE 2:
            // BA983210
            // CDEF4567

            tmp1 = Avx2.Permute2x128(lo, hi, 0x31);     // 89AB7654
            tmp1 = Avx2.Shuffle(tmp1, ShufRev4);        // BA984567
            lo = Avx2.Permute2x128(lo, tmp1, 0x30);     // BA983210
            hi = Avx2.Permute2x128(hi, tmp1, 0x02);     // CDEF4567
            CompareSwap(ref lo, ref hi);

            // STAGE 3:
            // DC985410
            // EFAB6723

            Swap(ref lo, ref hi, AlternatingMaskHi64);  // H:CD984510 - L:BAEF3267
            lo = Avx2.Shuffle(lo, 0b01001011);          // 
            hi = Avx2.Shuffle(hi, 0b10110100);          // 
            Swap(ref lo, ref hi, Avx2.CompareGreaterThan(hi, lo));

            // STAGE 4:
            // ECA86420
            // FDB97531

            // Final stage: restore order.

            Avx.Store(data, lo);
            Avx.Store(data + 8, hi);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static void CompareSwap(ref V lo, ref V hi) {
            var r = Avx2.CompareGreaterThan(hi, lo);
            var t = Avx2.BlendVariable(lo, hi, r);
            r = Avx2.Xor(r, Complement);
            lo = Avx2.BlendVariable(lo, hi, r);
            hi = t;
        }

        /// <summary>
        /// Swaps elements of <paramref name="lo"/> and <paramref name="hi"/> where <paramref name="mask"/> is 1. 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static void Swap(ref V lo, ref V hi, V mask) {
            var t = Avx2.BlendVariable(lo, hi, mask);
            lo = Avx2.BlendVariable(lo, hi, Avx2.Xor(mask, Complement));
            hi = t;
        }

        /// <summary>
        /// Validates <see cref="Sort(int*)"/> by exploiting theorem Z of section 5.3.4: it is
        /// sufficient to check that all 0-1 sequences (2^16 of them) are sorted by the network.
        /// </summary>
        public static unsafe void Check() {
            var bits = new int[16];
            
            fixed (int* b = bits) {
                for (int i = 0; i < 1 << 16; ++i) {
                    for (int j = i, k = 0; k < 16; ++k, j >>= 1)
                        bits[k] = j & 1;
                    Sort(b);
                    if (!IsSorted(bits))
                        throw new InvalidOperationException($"Sorting failed for bit pattern {i:X4}.");
                }
            }
        }

        /// <summary>
        /// Test method for debugging instruction sequences.
        /// </summary>
        public static unsafe void Test() {
            var data = new int[16];
            for (int i = 0; i < 16; ++i) data[i] = i;
            fixed (int* p = data)
                Sort(p);
        }

        public static bool IsSorted(int[] data) {
            for (int i = 1; i < data.Length; ++i)
                if (data[i] < data[i - 1])
                    return false;
            return true;
        }
    }
}
