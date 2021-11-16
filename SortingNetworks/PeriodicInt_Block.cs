using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace SortingNetworks
{
    using V = Vector256<int>;

    partial class PeriodicInt
    {
        /// <summary>
        /// Used to implement a single compare-swap phase for N elements; this processes 32 items at a time.
        /// Range <c>[b, b+16)</c> is compared/exchanged with reversed range <c>[e-16, e).</c>.
        /// </summary>
        /// <remarks>
        /// TODO: Should use aligned loads and non-temporal stores once it's possible to allocate aligned storage in .NET.
        /// </remarks>
        /// <param name="b">Start of the block.</param>
        /// <param name="e">One past the end of the block.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public unsafe void Phase_N_32(int* b, int* e) {
            V m0, m1;

            // Low half.
            var v0 = Avx.LoadVector256(b + 0);
            var v1 = Avx.LoadVector256(b + 8);

            // High half. Interleave loads with reversing.
            var v2 = Avx2.PermuteVar8x32(Avx.LoadVector256(e - 16), ReversePermutation);
            var v3 = Avx2.PermuteVar8x32(Avx.LoadVector256(e -  8), ReversePermutation);

            // Comparisons, interleaved with stores.  Min/max have throughput of 0.5, so we can execute two at once.
            // Use m0 and m1 to exploit the fact that min/max have a throughput < 1.
            m0 = Avx2.Min(v0, v3);
            m1 = Avx2.PermuteVar8x32(Avx2.Max(v0, v3), ReversePermutation);
            Avx.Store(b + 0, m0);
            Avx.Store(e - 8, m1);

            m0 = Avx2.Min(v1, v2);
            m1 = Avx2.PermuteVar8x32(Avx2.Max(v1, v2), ReversePermutation);
            Avx.Store(b + 8, m0);
            Avx.Store(e - 16, m1);
        }

        /// <summary>
        /// Block for sorting one vector of 32 elements (four registers).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void Block_32_1(int p, ref V _v0, ref V _v1, ref V _v2, ref V _v3) {
            V v0 = _v0, v1 = _v1, v2, v3, m0, m1;
            
            v2 = Avx2.PermuteVar8x32(_v2, ReversePermutation);
            v3 = Avx2.PermuteVar8x32(_v3, ReversePermutation);
            m0 = Avx2.Max(v0, v3);
            m1 = Avx2.Max(v1, v2);
            v0 = Avx2.Min(v0, v3);
            v1 = Avx2.Min(v1, v2);
            v2 = Avx2.PermuteVar8x32(m1, ReversePermutation);
            v3 = Avx2.PermuteVar8x32(m0, ReversePermutation);
            if (p == 1)
                goto done;

            Block_16_1(p - 1, ref v0, ref v1);
            Block_16_1(p - 1, ref v2, ref v3);

        done:
            _v0 = v0; _v1 = v1;
            _v2 = v2; _v3 = v3;
        }

        /// <summary>
        /// Block for sorting one vector of 16 elements (two registers).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Block_16_1(int p, ref V _v0, ref V _v1) {
            V v0 = _v0, v1, m;

            v1 = Avx2.PermuteVar8x32(_v1, ReversePermutation);
            m = Avx2.Max(v0, v1);
            v0 = Avx2.Min(v0, v1);
            v1 = Avx2.PermuteVar8x32(m, ReversePermutation);
            if (p == 1)
                goto done;

            Block_8_1(p - 1, ref v0);
            Block_8_1(p - 1, ref v1);
            
        done:
            _v0 = v0;
            _v1 = v1;
        }

        /// <summary>
        /// Block for sorting one vector of 8 elements.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Block_8_1(int p, ref V _v) {
            V v0 = _v, v1, m;

            // PHASE1:
            // 76543210
            // 01234567

            v1 = Avx2.PermuteVar8x32(v0, ReversePermutation);
            m = Avx2.CompareGreaterThan(v0, v1);
            m = Avx2.Xor(m, AlternatingMaskHi128);
            v0 = Avx2.BlendVariable(v0, v1, m);
            if (p == 1)
                goto done;

            Block_4_2(p - 1, ref v0);
        
        done:
            _v = v0;
        }

        /// <summary>
            /// Block for sorting 2 independent vectors of 4 elements each.
            /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Block_4_2(int p, ref V _v) {
            V v0 = _v, v1, m;

            // PHASE1:
            // 3210 (INPUT, same in both lanes)
            // 0123

            v1 = Avx2.Shuffle(v0, 0x1B);        // 0123
            m = Avx2.CompareGreaterThan(v0, v1);
            m = Avx2.Xor(m, AlternatingMaskHi64);
            v0 = Avx2.BlendVariable(v0, v1, m);
            if (p == 1)
                goto done;

            // PHASE2:
            // 3210
            // 2301

            v1 = Avx2.Shuffle(v0, 0b10110001);  // 2301
            m = Avx2.CompareGreaterThan(v0, v1);
            m = Avx2.Xor(m, AlternatingMaskHi32);
            v0 = Avx2.BlendVariable(v0, v1, m);
        
        done:
            _v = v0;
        }
    }
}
