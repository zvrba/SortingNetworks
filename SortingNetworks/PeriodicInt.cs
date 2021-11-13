using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace SortingNetworks
{
    using V = Vector256<int>;

    /// <summary>
    /// Provides methods for sorting integer arrays of lengths that are a power of two.  Invoking public members with
    /// arrays that are of shorter length will result in UNDEFINED BEHAVIOR (data corruption, crash).
    /// </summary>
    public partial class PeriodicInt
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

        /// <summary>
        /// In-place sorts 16 elements starting at <paramref name="data"/>.
        /// </summary>
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

        /// <summary>
        /// In-place sorts 32 elements starting at <paramref name="data"/>.
        /// </summary>
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
    }
}
