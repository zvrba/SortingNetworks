using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace SortingNetworks
{
    using V = Vector256<int>;

    sealed unsafe class IntSorter : UnsafeSort<int>
    {
        // TODO: Place these inside own unsafe struct.
        readonly V Zero;                    // 00000000
        readonly V Complement;              // FFFFFFFF
        readonly V AlternatingMaskLo128;    // 0000FFFF
        readonly V AlternatingMaskHi128;    // FFFF0000
        readonly V AlternatingMaskHi64;     // FF00FF00
        readonly V AlternatingMaskHi32;     // F0F0F0F0
        readonly V Max;                     // int.MaxValue in each element
        readonly V ReversePermutation;      // Input to VPERMD that reverses all 8 ints
        readonly V[] CountMask;             // For loading 1-8 elements. VPALIGNR requires an immediate constant, which kills perf.

        internal IntSorter(int maxLength) {
            Zero = V.Zero;
            Complement = Avx2.CompareEqual(Zero, Zero);
            AlternatingMaskHi128 = Vector256.Create(0L, 0L, -1L, -1L).AsInt32();
            AlternatingMaskLo128 = Vector256.Create(-1L, -1L, 0L, 0L).AsInt32();
            AlternatingMaskHi64 = Avx2.Xor(Complement, Avx2.ShiftRightLogical128BitLane(Complement, 8));
            AlternatingMaskHi32 = Avx2.Xor(Complement.AsInt64(), Avx2.ShiftRightLogical(Complement.AsInt64(), 32)).AsInt32();
            Max = Vector256.Create(int.MaxValue);
            ReversePermutation = Vector256.Create(7, 6, 5, 4, 3, 2, 1, 0);
            CountMask = new V[8];
            CountMask[0] = Complement;
            CountMask[1] = Vector256.Create(-1, 0, 0, 0, 0, 0, 0, 0);
            CountMask[2] = Vector256.Create(-1, -1, 0, 0, 0, 0, 0, 0);
            CountMask[3] = Vector256.Create(-1, -1, -1, 0, 0, 0, 0, 0);
            CountMask[4] = Vector256.Create(-1, -1, -1, -1, 0, 0, 0, 0);
            CountMask[5] = Vector256.Create(-1, -1, -1, -1, -1, 0, 0, 0);
            CountMask[6] = Vector256.Create(-1, -1, -1, -1, -1, -1, 0, 0);
            CountMask[7] = Vector256.Create(-1, -1, -1, -1, -1, -1, -1, 0);

            if (maxLength <= 8) {
                MinLength = 4;
                MaxLength = 8;
                Sorter = Sort8;
            }
            else if (maxLength <= 16) {
                MinLength = 9;
                MaxLength = 16;
                Sorter = Sort16;
            }
            else {
                MinLength = 16;
                MaxLength = 1 << 24;
                Sorter = Sort;
                if (maxLength > MaxLength)
                    throw new ArgumentOutOfRangeException("Maximum supported length is 2^24.");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe void Sort8(int* data, int c) {
            var v = Load8(data, c);
            Block8(2, ref v);
            Block8(3, ref v);
            Block8(3, ref v);
            Store8(data, v, c);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe void Sort16(int* data, int c) {
            var v0 = Avx.LoadVector256(data);
            var v1 = Load8(data + 8, c - 8);
            Block16(2, ref v0, ref v1);
            Block16(3, ref v0, ref v1);
            Block16(4, ref v0, ref v1);
            Block16(4, ref v0, ref v1);
            Avx.Store(data, v0);
            Store8(data + 8, v1, c - 8);
        }

        unsafe void Sort(int* data, int c) {
            var (upsize, log2c) = UpSize(c);
            for (int i = 0; i < log2c; ++i)
                Block(i + 2 < log2c ? i + 2 : log2c, data, c, upsize);

            static (int upsize, int log2c) UpSize(int size) {
                --size;
                size |= size >> 1;
                size |= size >> 2;
                size |= size >> 4;
                size |= size >> 8;
                size |= size >> 16;

                var upsize = size + 1;
                int log2c = -1;
                for (size = upsize; size > 0; ++log2c, size >>= 1)
                    ;

                return (upsize, log2c);
            }
        }

        // b and e point to the true range to be sorted.  upsize is (e-b) rounded up to a power of two.
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe void Block(int p, int* b, int c, int upsize) {
            int split = 1;
            for (; p > 0 && upsize >= 8; --p, split *= 2, upsize /= 2) {
                for (int i = 0, sb = 0; i < split && sb < c; ++i, sb += upsize) {
                    var sc = upsize;
                    if (sb + upsize > c)
                        sc = c - sb;
                    Phase(p, b + sb, sc, upsize);
                }
            }
        }

        // b points to block start, c is the actual # of elements in the block and upsize is c rounded up to power of two.
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        unsafe void Phase(int p, int* b, int c, int upsize) {
            if (upsize > 8) {
                var i0 = (upsize - c) >> 3;
                var c0 = (upsize - c) & 7;

                int* e = b + upsize - 8 * (i0 + 1);
                b += 8 * i0;

                if (c0 != 0 && b < e) {
                    PhaseStep(1, b, e, 16 - c0);
                    b += 8;
                    e -= 8;
                }

                for (;  b < e; b += 8, e -= 8) 
                    PhaseStep(b, e);
            }
            else {
                Block8(p, b, c);
            }
        }

        // Full size (16) compare-exchange.
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        unsafe void PhaseStep(int* lo, int* hi) {
            var v0 = Avx.LoadVector256(lo);
            var v1 = Avx.LoadVector256(hi);
            Block16(1, ref v0, ref v1);
            Avx.Store(lo, v0);
            Avx.Store(hi, v1);
        }

        // No inlining; executed at most once.
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe void PhaseStep(int p, int* lo, int* hi, int c) {
            var v0 = Avx.LoadVector256(lo);
            var v1 = Load8(hi, c - 8);
            Block16(p, ref v0, ref v1);
            Avx.Store(lo, v0);
            Store8(hi, v1, c - 8);
        }

        // No inlining; executed at most once.
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe void Block8(int p, int* b, int c) {
            var v = Load8(b, c);
            Block8(p, ref v);
            Store8(b, v, c);
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
            var m = CountMask[c & 7];
            return Avx2.BlendVariable(Max, Avx2.MaskLoad(v, m), m);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        unsafe void Store8(int* a, V v, int c) {
            var m = CountMask[c & 7];
            Avx2.MaskStore(a, m, v);
        }
    }
}
