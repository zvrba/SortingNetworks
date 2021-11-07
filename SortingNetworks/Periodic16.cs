using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace SortingNetworks
{
    using V = Vector256<int>;

    /// <summary>
    /// 16-element periodic sorting network with optimization described after Theorem 2 in Dowd et al.: "The Periodic Balanced
    /// Sorting Network", JACM Vol. 36, No. 4, October 1989, pp. 738-757.  This implementation is NOT branchless, but performs
    /// fewer operations.  Comments in the code reflect the names from the paper.
    /// </summary>
    static class Periodic16
    {
        // All zeros
        static readonly V Zero = V.Zero;

        // All ones
        static readonly V Complement = Avx2.CompareEqual(Zero, Zero);

        // FFFF0000
        static readonly V AlternatingMaskHi128 = Vector256.Create(0L, 0L, -1L, -1L).AsInt32();

        // 0000FFFF
        static readonly V AlternatingMaskLo128 = Vector256.Create(-1L, -1L, 0L, 0L).AsInt32();
        
        // FF00FF00 (1 digit = 32 bits)
        static readonly V AlternatingMaskHi64 = Avx2.Xor(
            Complement,
            Avx2.ShiftRightLogical128BitLane(Complement, 8));
        
        // F0F0F0F0
        static readonly V AlternatingMaskLo32 = Avx2.Xor(
            Complement.AsInt64(),
            Avx2.ShiftLeftLogical(Complement.AsInt64(), 32)
        ).AsInt32();


        public static unsafe void Sort(int* data) {
            var lo = Avx.LoadVector256(data);
            var hi = Avx.LoadVector256(data + 8);

            Block(2, ref lo, ref hi);
            Block(3, ref lo, ref hi);
            Block(4, ref lo, ref hi);
            Block(4, ref lo, ref hi);

            Avx.Store(data, lo);
            Avx.Store(data + 8, hi);
        }

        /// <summary>
        /// Performs the operations in a single, potentially partial, block.  The integers in each half (vector parameter)
        /// are ordered from least to most significant bits.
        /// </summary>
        /// <param name="p">Phase to stop at; must be 1, 2, 3 or 4.  Unchecked; 4 or any other value will run the whole block.</param>
        /// <param name="lo">Low half of elements to sort.</param>
        /// <param name="hi">High half of elements to sort.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void Block(int p, ref V lo, ref V hi) {
            V tmp1, tmp2;

            // INPUT:
            // 76543210
            // FEDCBA98
            // lo, hi are intermediate results after each stage and input to next one.

            // PHASE 1:
            // 76543210
            // 89ABCDEF

            tmp1 = Avx2.Shuffle(hi, 0x1B);              // CDEF89AB
            hi = Avx2.Permute2x128(tmp1, tmp1, 0x01);   // 89ABCDEF
            Swap(ref lo, ref hi, Avx2.CompareGreaterThan(hi, lo));
            if (p == 1) {
                hi = Avx2.Permute2x128(hi, hi, 0x01);
                hi = Avx2.Shuffle(hi, 0x1B);
                return;
            }

            // PHASE 2:
            // BA983210
            // CDEF4567

            tmp1 = Avx2.Permute2x128(lo, hi, 0x31);     // 89AB7654
            tmp1 = Avx2.Shuffle(tmp1, 0x1B);            // BA984567
            lo = Avx2.Permute2x128(lo, tmp1, 0x30);     // BA983210
            hi = Avx2.Permute2x128(hi, tmp1, 0x02);     // CDEF4567
            Swap(ref lo, ref hi, Avx2.CompareGreaterThan(hi, lo));
            if (p == 2) {
                hi = Avx2.Shuffle(hi, 0x1B);            // FEDC7654
                tmp1 = Avx2.Permute2x128(lo, hi, 0x21); // 7654BA98
                Swap(ref lo, ref tmp1, AlternatingMaskLo128);
                Swap(ref hi, ref tmp1, AlternatingMaskHi128);
                return;
            }

            // PHASE 3:
            // DC985410
            // EFAB6723

            Swap(ref lo, ref hi, AlternatingMaskHi64);  // L:CD984510 - H:BAEF3267
            lo = Avx2.Shuffle(lo, 0b01001011);          // 
            hi = Avx2.Shuffle(hi, 0b10110100);          // 
            Swap(ref lo, ref hi, Avx2.CompareGreaterThan(hi, lo));
            if (p == 3) {
                hi = Avx2.Shuffle(hi, 0b10110001);      // FEBA7632
                tmp1 = Avx2.UnpackLow(lo.AsInt64(),
                    hi.AsInt64()).AsInt32();            // BA983210
                tmp2 = Avx2.UnpackHigh(lo.AsInt64(),
                    hi.AsInt64()).AsInt32();            // FEDC7654
                goto fixup;
            }

            // PHASE 4:
            // ECA86420
            // FDB97531

            Swap(ref lo, ref hi, AlternatingMaskLo32);  // L:ECA86420 - H:DF9B5713
            hi = Avx2.Shuffle(hi, 0b10110001);
            Swap(ref lo, ref hi, Avx2.CompareGreaterThan(hi, lo));

            // Final stage: restore order.

            tmp1 = Avx2.UnpackLow(lo, hi);
            tmp2 = Avx2.UnpackHigh(lo, hi);

        fixup:
            lo = Avx2.Permute2x128(tmp1, tmp2, 0x20);
            hi = Avx2.Permute2x128(tmp1, tmp2, 0x31);
        }

        /// <summary>
        /// Swaps elements of <paramref name="lo"/> and <paramref name="hi"/> where <paramref name="mask"/> is 1. 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static void Swap(ref V lo, ref V hi, V mask) {
            var t = Avx2.BlendVariable(lo, hi, mask);
            lo = Avx2.BlendVariable(hi, lo, mask);
            hi = t;
        }
    }
}
