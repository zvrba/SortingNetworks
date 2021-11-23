using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace SortingNetworks
{
    using V = Vector256<float>;
    using VI = Vector256<int>;

    sealed unsafe class FloatSorter : UnsafeSort<float>
    {
        readonly V Zero;                    // 00000000
        readonly V Complement;              // FFFFFFFF
        readonly V AlternatingMaskLo128;    // 0000FFFF
        readonly V AlternatingMaskHi128;    // FFFF0000
        readonly V AlternatingMaskHi64;     // FF00FF00
        readonly V AlternatingMaskHi32;     // F0F0F0F0
        readonly V Max;                     // int.MaxValue in each element
        readonly VI ReversePermutation;     // Input to VPERMD that reverses all 8 ints
        readonly V[] CountMask;             // For loading 1-8 elements. VPALIGNR requires an immediate constant, which kills perf.

        internal FloatSorter(int maxLength) {
            Zero = V.Zero;
            Complement = Vector256.Create(-1).AsSingle();
            AlternatingMaskHi128 = Vector256.Create(0L, 0L, -1L, -1L).AsSingle();
            AlternatingMaskLo128 = Vector256.Create(-1L, -1L, 0L, 0L).AsSingle();
            AlternatingMaskHi64 = Avx2.Xor(Complement.AsByte(), Avx2.ShiftRightLogical128BitLane(Complement.AsByte(), 8)).AsSingle();
            AlternatingMaskHi32 = Avx2.Xor(Complement.AsInt64(), Avx2.ShiftRightLogical(Complement.AsInt64(), 32)).AsSingle();
            Max = Vector256.Create(float.PositiveInfinity);
            ReversePermutation = Vector256.Create(7, 6, 5, 4, 3, 2, 1, 0);
            CountMask = new V[8];
            CountMask[0] = Complement;
            CountMask[1] = Vector256.Create(-1, 0, 0, 0, 0, 0, 0, 0).AsSingle();
            CountMask[2] = Vector256.Create(-1, -1, 0, 0, 0, 0, 0, 0).AsSingle();
            CountMask[3] = Vector256.Create(-1, -1, -1, 0, 0, 0, 0, 0).AsSingle();
            CountMask[4] = Vector256.Create(-1, -1, -1, -1, 0, 0, 0, 0).AsSingle();
            CountMask[5] = Vector256.Create(-1, -1, -1, -1, -1, 0, 0, 0).AsSingle();
            CountMask[6] = Vector256.Create(-1, -1, -1, -1, -1, -1, 0, 0).AsSingle();
            CountMask[7] = Vector256.Create(-1, -1, -1, -1, -1, -1, -1, 0).AsSingle();

            if (maxLength <= 8) {
                MinLength = 4;
                MaxLength = 8;
                Sorter = Sort8;
            } else if (maxLength <= 16) {
                MinLength = 9;
                MaxLength = 16;
                Sorter = Sort16;
            } else {
                MinLength = 16;
                MaxLength = 1 << 24;
                Sorter = Sort;
                if (maxLength > MaxLength)
                    throw new ArgumentOutOfRangeException("Maximum supported length is 2^24.");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe void Sort8(float* data, int c) {
            var v = Load8(data, c);
            Block8(2, ref v);
            Block8(3, ref v);
            Block8(3, ref v);
            Store8(data, v, c);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe void Sort16(float* data, int c) {
            var v0 = Avx.LoadVector256(data);
            var v1 = Load8(data + 8, c - 8);
            Block16(2, ref v0, ref v1);
            Block16(3, ref v0, ref v1);
            Block16(4, ref v0, ref v1);
            Block16(4, ref v0, ref v1);
            Avx.Store(data, v0);
            Store8(data + 8, v1, c - 8);
        }

        unsafe void Sort(float* data, int c) {
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
        unsafe void Block(int p, float* b, int c, int upsize) {
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
        unsafe void Phase(int p, float* b, int c, int upsize) {
            if (upsize > 8) {
                var i0 = (upsize - c) >> 3;
                var c0 = (upsize - c) & 7;

                float* e = b + upsize - 8 * (i0 + 1);
                b += 8 * i0;

                if (c0 != 0 && b < e) {
                    PhaseStep(1, b, e, 16 - c0);
                    b += 8;
                    e -= 8;
                }

                for (; b < e; b += 8, e -= 8)
                    PhaseStep(b, e);
            } else {
                Block8(p, b, c);
            }
        }

        // Full size (16) compare-exchange.
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        unsafe void PhaseStep(float* lo, float* hi) {
            var v0 = Avx.LoadVector256(lo);
            var v1 = Avx.LoadVector256(hi);
            Block16(1, ref v0, ref v1);
            Avx.Store(lo, v0);
            Avx.Store(hi, v1);
        }

        // No inlining; executed at most once.
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe void PhaseStep(int p, float* lo, float* hi, int c) {
            var v0 = Avx.LoadVector256(lo);
            var v1 = Load8(hi, c - 8);
            Block16(p, ref v0, ref v1);
            Avx.Store(lo, v0);
            Store8(hi, v1, c - 8);
        }

        // No inlining; executed at most once.
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe void Block8(int p, float* b, int c) {
            var v = Load8(b, c);
            Block8(p, ref v);
            Store8(b, v, c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        unsafe void Block16(int p, ref V _v0, ref V _v1) {
            V v0 = _v0, v1, m;

            v1 = Avx2.PermuteVar8x32(_v1, ReversePermutation);
            m = Avx.Max(v0, v1);
            v0 = Avx.Min(v0, v1);
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
            m = Avx.Compare(v0, v1, FloatComparisonMode.OrderedGreaterThanNonSignaling);
            m = Avx.Xor(m, AlternatingMaskHi128);
            v0 = Avx.BlendVariable(v0, v1, m);
            if (--p == 0)
                goto done;

            // COMPARE / SWAP PHASE
            // 76543210
            // 45670123

            v1 = Avx2.Shuffle(v0, v0, 0x1B);
            m = Avx.Compare(v0, v1, FloatComparisonMode.OrderedGreaterThanNonSignaling);
            m = Avx.Xor(m, AlternatingMaskHi64);
            v0 = Avx.BlendVariable(v0, v1, m);
            if (--p == 0)
                goto done;

            // COMPARE / SWAP PHASE
            // 76543210
            // 67452301

            v1 = Avx.Shuffle(v0, v0, 0b10110001);
            m = Avx.Compare(v0, v1, FloatComparisonMode.OrderedGreaterThanNonSignaling);
            m = Avx.Xor(m, AlternatingMaskHi32);
            v0 = Avx.BlendVariable(v0, v1, m);

        done:
            v = v0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        unsafe V Load8(float* v, int c) {
            var m = CountMask[c & 7];
            return Avx.BlendVariable(Max, Avx.MaskLoad(v, m), m);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        unsafe void Store8(float* a, V v, int c) {
            var m = CountMask[c & 7];
            Avx.MaskStore(a, m, v);
        }
    }
}
