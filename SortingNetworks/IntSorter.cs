using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace SortingNetworks
{
    using V = Vector256<int>;

    class IntSorter : UnsafeSort<int>
    {
        readonly V Zero;                    // 00000000
        readonly V Complement;              // FFFFFFFF
        readonly V AlternatingMaskLo128;    // 0000FFFF
        readonly V AlternatingMaskHi128;    // FFFF0000
        readonly V AlternatingMaskHi64;     // FF00FF00
        readonly V AlternatingMaskHi32;     // F0F0F0F0
        readonly V Max;                     // int.MaxValue in each element
        readonly V ReversePermutation;      // Input to VPERMD that reverses all 8 ints

        internal IntSorter() : base(null, 1, int.MaxValue) {
            Zero = V.Zero;
            Complement = Avx2.CompareEqual(Zero, Zero);
            AlternatingMaskHi128 = Vector256.Create(0L, 0L, -1L, -1L).AsInt32();
            AlternatingMaskLo128 = Vector256.Create(-1L, -1L, 0L, 0L).AsInt32();
            AlternatingMaskHi64 = Avx2.Xor(Complement, Avx2.ShiftRightLogical128BitLane(Complement, 8));
            AlternatingMaskHi32 = Avx2.Xor(Complement.AsInt64(), Avx2.ShiftRightLogical(Complement.AsInt64(), 32)).AsInt32();
            Max = Vector256.Create(int.MaxValue);
            ReversePermutation = Vector256.Create(7, 6, 5, 4, 3, 2, 1, 0);
        }

        public override unsafe void Sort(int* data) {
            throw new NotImplementedException();
        }

        public override unsafe void Sort(int* data, int c) {
            throw new NotImplementedException();
        }

        // b and e point to the true range to be sorted.  upsize is (e-b) rounded up to a power of two.
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe void Block(int p, int* b, int* e, int upsize) {
            int split = 1;
            for (; p > 0 && upsize >= 8; --p, split *= 2, upsize /= 2) {
                for (int i = 0; i < split; ++i) {
                    var pb = b + i * upsize;
                    if (pb >= e)    // We're out of bounds of the array, no more work to do in this inner loop.
                        break;

                    var pe = pb + upsize;
                    var c = (int)(pe > e ? e - pb : pe - pb);
                    if (pe > e) pe = e;
                    Phase(p, pb, c, upsize);
                }
            }
        }

        // b points to block start, c is the actual # of elements in the block and upsize is c rounded up to power of two.
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        unsafe void Phase(int p, int* b, int c, int upsize) {
            if (upsize > 16) {
                var i0 = (upsize - c) >> 3;
                var r0 = (upsize - c) & 7;
            }
            else if (upsize > 8) {
                var v0 = Avx.LoadVector256(b);
                var v1 = Load8(b + 8, c - 8);
                Block16(p, ref v0, ref v1);
                Avx.Store(b, v0);
                Store8(b + 8, v1, c - 8);
            }
            else {
                var v = Load8(b, c);
                Block8(p, ref v);
                Store8(b, v, c);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        unsafe void Block16(int p, ref V _v0, ref V _v1) {
            V v0 = _v0, v1, m;

            v1 = Avx2.PermuteVar8x32(_v1, ReversePermutation);
            m = Avx2.Max(v0, v1);
            v0 = Avx2.Min(v0, v1);
            v1 = Avx2.PermuteVar8x32(m, ReversePermutation);
            if (--p == 0)
                goto done;

            Block8(p, ref v0);
            Block8(p, ref v1);

        done:
            _v0 = v0; _v1 = v1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        void Block8(int p, ref V v) {
            V v0 = v, v1, m;

            // COMPARE / SWAP PHASE
            // 76543210
            // 01234567

            v1 = Avx2.PermuteVar8x32(v0, ReversePermutation);
            m = Avx2.CompareGreaterThan(v0, v1);
            m = Avx2.Xor(m, AlternatingMaskHi128);
            v0 = Avx2.BlendVariable(v0, v1, m);
            if (--p == 0)
                goto done;

            // COMPARE / SWAP PHASE
            // 76543210
            // 45670123

            v1 = Avx2.Shuffle(v0, 0x1B);
            m = Avx2.CompareGreaterThan(v0, v1);
            m = Avx2.Xor(m, AlternatingMaskHi64);
            v0 = Avx2.BlendVariable(v0, v1, m);
            if (--p == 0)
                goto done;

            // COMPARE / SWAP PHASE
            // 76543210
            // 67452301

            v1 = Avx2.Shuffle(v0, 0b10110001);
            m = Avx2.CompareGreaterThan(v0, v1);
            m = Avx2.Xor(m, AlternatingMaskHi32);
            v0 = Avx2.BlendVariable(v0, v1, m);

        done:
            v = v0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        unsafe V Load8(int* v, int c) {
            var m = Avx2.AlignRight(AlternatingMaskLo128, Complement, (byte)(8 - c));
            return Avx2.BlendVariable(Max, Avx2.MaskLoad(v, m), m);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        unsafe void Store8(int* a, V v, int c) {
            var m = Avx2.AlignRight(AlternatingMaskLo128, Complement, (byte)(8 - c));
            Avx2.MaskStore(a, m, v);
        }
    }
}
