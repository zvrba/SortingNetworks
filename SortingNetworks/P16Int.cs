using System;
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
    class P16Int
    {
        readonly V Zero;                    // 00000000
        readonly V Complement;              // FFFFFFFF
        readonly V AlternatingMaskHi128;    // FFFF0000
        readonly V AlternatingMaskLo128;    // 0000FFFF
        readonly V AlternatingMaskHi64;     // FF00FF00
        readonly V AlternatingMaskLo32;     // F0F0F0F0

        public P16Int() {
            Zero = V.Zero;
            Complement = Avx2.CompareEqual(Zero, Zero);
            AlternatingMaskHi128 = Vector256.Create(0L, 0L, -1L, -1L).AsInt32();
            AlternatingMaskLo128 = Vector256.Create(-1L, -1L, 0L, 0L).AsInt32();
            AlternatingMaskHi64 = Avx2.Xor(Complement, Avx2.ShiftRightLogical128BitLane(Complement, 8));
            AlternatingMaskLo32 = Avx2.Xor(Complement.AsInt64(), Avx2.ShiftLeftLogical(Complement.AsInt64(), 32)).AsInt32();
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
