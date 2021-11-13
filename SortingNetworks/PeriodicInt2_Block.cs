using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace SortingNetworks
{
    using V = Vector256<int>;

    public partial class PeriodicInt2
    {
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
            if (p == 1) {
                _v = v0;
                return;
            }

            // PHASE2:
            // 3210
            // 2301

            v1 = Avx2.Shuffle(v0, 0b10110001);  // 2301
            m = Avx2.CompareGreaterThan(v0, v1);
            m = Avx2.Xor(m, AlternatingMaskHi32);
            _v = Avx2.BlendVariable(v0, v1, m);
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
