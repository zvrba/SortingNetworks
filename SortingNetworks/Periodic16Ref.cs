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
        // F0F0F0F0
        static readonly V AlternatingMaskLo32 = Avx2.Xor(
            Complement.AsInt64(),
            Avx2.ShiftLeftLogical(Complement.AsInt64(), 32)
        ).AsInt32();

        /// <summary>
        /// In-place sorting of 16 elements starting at <paramref name="data"/>.
        /// </summary>
        /// <param name="data"></param>
        public static unsafe void Sort(int* data) {
            var lo = Avx.LoadVector256(data);
            var hi = Avx.LoadVector256(data + 8);

            Step(ref lo, ref hi);
            Step(ref lo, ref hi);
            Step(ref lo, ref hi);
            Step(ref lo, ref hi);

            Avx.Store(data, lo);
            Avx.Store(data + 8, hi);
        }

        /// <summary>
        /// Test method for debugging instruction sequences.
        /// </summary>
        public static unsafe void Test() {
            var data = new int[16];
            for (int i = 0; i < 16; ++i) data[i] = i;
            fixed (int* p = data) {
                var lo = Avx.LoadVector256(p);
                var hi = Avx.LoadVector256(p + 8);
                Step(ref lo, ref hi);
            }
        }

        /// <summary>
        /// One step of the sorting network for 16 elements.  Must be iterated 4 times.
        /// </summary>
        /// <param name="data"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static void Step(ref V lo, ref V hi) {
            V tmp1, tmp2;

            // lo, hi are intermediate results after each stage and input to next one.

            // STAGE 1:
            // 76543210
            // 89ABCDEF

            tmp1 = Avx2.Shuffle(hi, 0x1B);              // CDEF89AB
            hi = Avx2.Permute2x128(tmp1, tmp1, 1);      // 89ABCDEF
            Swap(ref lo, ref hi, Avx2.CompareGreaterThan(hi, lo));

            // STAGE 2:
            // BA983210
            // CDEF4567

            tmp1 = Avx2.Permute2x128(lo, hi, 0x31);     // 89AB7654
            tmp1 = Avx2.Shuffle(tmp1, 0x1B);            // BA984567
            lo = Avx2.Permute2x128(lo, tmp1, 0x30);     // BA983210
            hi = Avx2.Permute2x128(hi, tmp1, 0x02);     // CDEF4567
            Swap(ref lo, ref hi, Avx2.CompareGreaterThan(hi, lo));

            // STAGE 3:
            // DC985410
            // EFAB6723

            Swap(ref lo, ref hi, AlternatingMaskHi64);  // L:CD984510 - H:BAEF3267
            lo = Avx2.Shuffle(lo, 0b01001011);          // 
            hi = Avx2.Shuffle(hi, 0b10110100);          // 
            Swap(ref lo, ref hi, Avx2.CompareGreaterThan(hi, lo));

            // STAGE 4:
            // ECA86420
            // FDB97531

            Swap(ref lo, ref hi, AlternatingMaskLo32);  // L:ECA86420 - H:DF9B5713
            hi = Avx2.Shuffle(hi, 0b10110001);
            Swap(ref lo, ref hi, Avx2.CompareGreaterThan(hi, lo));

            // Final stage: restore order.

            tmp1 = Avx2.UnpackLow(lo, hi);
            tmp2 = Avx2.UnpackHigh(lo, hi);
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
