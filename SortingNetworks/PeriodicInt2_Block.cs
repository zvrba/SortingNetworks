﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace SortingNetworks
{
    using V = Vector256<int>;

    public partial class PeriodicInt2
    {
        /// <summary>
        /// Block for sorting one vector of 16 elements (two registers).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        void Block_16_1(int p, ref V _v0, ref V _v1) {
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
        void Block_8_1(int p, ref V _v) {
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
        void Block_4_2(int p, ref V _v) {
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