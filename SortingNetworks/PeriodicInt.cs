﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace SortingNetworks
{
    using V = Vector256<int>;

    /// <summary>
    /// Building block for a periodic sorting network of 16 elements or more.  A single block consists of up to 4 phases
    /// as described in Dowd et al.
    /// </summary>
    public class PeriodicInt
    {
        readonly V Zero;                    // 00000000
        readonly V Complement;              // FFFFFFFF
        readonly V AlternatingMaskHi128;    // FFFF0000
        readonly V AlternatingMaskLo128;    // 0000FFFF
        readonly V AlternatingMaskHi64;     // FF00FF00
        readonly V AlternatingMaskLo32;     // F0F0F0F0

        public PeriodicInt() {
            Zero = V.Zero;
            Complement = Avx2.CompareEqual(Zero, Zero);
            AlternatingMaskHi128 = Vector256.Create(0L, 0L, -1L, -1L).AsInt32();
            AlternatingMaskLo128 = Vector256.Create(-1L, -1L, 0L, 0L).AsInt32();
            AlternatingMaskHi64 = Avx2.Xor(Complement, Avx2.ShiftRightLogical128BitLane(Complement, 8));
            AlternatingMaskLo32 = Avx2.Xor(Complement.AsInt64(), Avx2.ShiftLeftLogical(Complement.AsInt64(), 32)).AsInt32();
        }

        public unsafe void Sort16(int* data) {
            var lo = Avx.LoadVector256(data);
            var hi = Avx.LoadVector256(data + 8);

            Block16(2, ref lo, ref hi);
            Block16(3, ref lo, ref hi);
            Block16(4, ref lo, ref hi);
            Block16(4, ref lo, ref hi);

            Avx.Store(data, lo);
            Avx.Store(data + 8, hi);
        }

        public unsafe void Sort32(int* data) {
            var v0 = Avx.LoadVector256(data + 0);
            var v1 = Avx.LoadVector256(data + 8);
            var v2 = Avx.LoadVector256(data + 16);
            var v3 = Avx.LoadVector256(data + 24);

            Block32(2, ref v0, ref v1, ref v2, ref v3);
            Block32(3, ref v0, ref v1, ref v2, ref v3);
            Block32(4, ref v0, ref v1, ref v2, ref v3);
            Block32(5, ref v0, ref v1, ref v2, ref v3);
            Block32(5, ref v0, ref v1, ref v2, ref v3);

            Avx.Store(data + 0, v0);
            Avx.Store(data + 8, v1);
            Avx.Store(data + 16, v2);
            Avx.Store(data + 24, v3);
        }

        /// <summary>
        /// Operations of a (potentially partial) 32-block.  The integers are ordered from the least significant element in
        /// <paramref name="v0"/> to the most significant element in <paramref name="v3"/>.
        /// </summary>
        /// <param name="p">Phase to stop at; must be 1-5.</param>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        void Block32(int p, ref V _v0, ref V _v1, ref V _v2, ref V _v3) {
            var v2 = Avx2.Permute2x128(_v2, _v2, 0x01);
            var v3 = Avx2.Permute2x128(_v3, _v3, 0x01);
            v2 = Avx2.Shuffle(v2, 0x1B);
            v3 = Avx2.Shuffle(v3, 0x1B);

            Swap(ref _v0, ref v3, Avx2.CompareGreaterThan(v3, _v0));    // 0-7  : 31:24
            Swap(ref _v1, ref v2, Avx2.CompareGreaterThan(v2, _v1));    // 8-15 : 23:16

            v2 = Avx2.Shuffle(v2, 0x1B);
            v3 = Avx2.Shuffle(v3, 0x1B);
            v2 = Avx2.Permute2x128(v2, v2, 0x01);
            v3 = Avx2.Permute2x128(v3, v3, 0x01);

            Block16(p - 1, ref _v0, ref _v1);
            Block16(p - 1, ref v2, ref v3);
            _v2 = v2; _v3 = v3;
        }

        /// <summary>
        /// Performs the operations of a single, potentially partial, 16-block.  The integers in each half (vector parameter)
        /// are ordered from least to most significant bits.
        /// </summary>
        /// <param name="p">Phase to stop at; must be 1, 2, 3 or 4.  Unchecked; 4 or any other value will run the whole block.</param>
        /// <param name="_lo">Low half of elements to sort.</param>
        /// <param name="_hi">High half of elements to sort.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        void Block16(int p, ref V _lo, ref V _hi) {
            V lo = _lo, hi = _hi;                       // Stack-allocated to eliminate unnecessary loads/stores to refs
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
                _lo = lo; _hi = hi;
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
                _lo = lo; _hi = hi;
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
            _lo = lo; _hi = hi;
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
