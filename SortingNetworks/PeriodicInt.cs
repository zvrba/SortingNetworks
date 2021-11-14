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
    /// <remarks>
    /// You're not expected to understand this code unless you have read the paper by Dowd et al.
    /// </remarks>
    public partial class PeriodicInt
    {
        public readonly V Zero;                    // 00000000
        public readonly V Complement;              // FFFFFFFF
        public readonly V AlternatingMaskLo128;    // 0000FFFF
        public readonly V AlternatingMaskHi128;    // FFFF0000
        public readonly V AlternatingMaskHi64;     // FF00FF00
        public readonly V AlternatingMaskHi32;     // F0F0F0F0
        public readonly V Max;                     // int.MaxValue in each element
        public readonly V ReversePermutation;      // Input to VPERMD that reverses all 8 ints 

        public PeriodicInt() {
            Zero = V.Zero;
            Complement = Avx2.CompareEqual(Zero, Zero);
            AlternatingMaskHi128 = Vector256.Create(0L, 0L, -1L, -1L).AsInt32();
            AlternatingMaskLo128 = Vector256.Create(-1L, -1L, 0L, 0L).AsInt32();
            AlternatingMaskHi64 = Avx2.Xor(Complement, Avx2.ShiftRightLogical128BitLane(Complement, 8));
            AlternatingMaskHi32 = Avx2.Xor(Complement.AsInt64(), Avx2.ShiftRightLogical(Complement.AsInt64(), 32)).AsInt32();
            Max = Vector256.Create(int.MaxValue);
            ReversePermutation = Vector256.Create(7, 6, 5, 4, 3, 2, 1, 0);
        }

        // This is the last size that can be reasonably inlined due to code size (> 1kB of straight-line code) and # of used registers.
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public unsafe void Sort32(int* data) {
            var v0 = Avx.LoadVector256(data + 0);
            var v1 = Avx.LoadVector256(data + 8);
            var v2 = Avx.LoadVector256(data + 16);
            var v3 = Avx.LoadVector256(data + 24);
            Block_32_1(2, ref v0, ref v1, ref v2, ref v3);
            Block_32_1(3, ref v0, ref v1, ref v2, ref v3);
            Block_32_1(4, ref v0, ref v1, ref v2, ref v3);
            Block_32_1(5, ref v0, ref v1, ref v2, ref v3);
            Block_32_1(5, ref v0, ref v1, ref v2, ref v3);
            Avx.Store(data + 0, v0);
            Avx.Store(data + 8, v1);
            Avx.Store(data + 16, v2);
            Avx.Store(data + 24, v3);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public unsafe void Sort32(int* data, int c) {
            throw new NotImplementedException("Sort32");
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public unsafe void Sort16(int* data) {
            var v0 = Avx.LoadVector256(data + 0);
            var v1 = Avx.LoadVector256(data + 8);
            Block_16_1(2, ref v0, ref v1);
            Block_16_1(3, ref v0, ref v1);
            Block_16_1(4, ref v0, ref v1);
            Block_16_1(4, ref v0, ref v1);
            Avx.Store(data + 0, v0);
            Avx.Store(data + 8, v1);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public unsafe void Sort16(int* data, int c) {
            throw new NotImplementedException("Sort16");
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public unsafe void Sort8(int* data) {
            var v = Avx.LoadVector256(data);
            Block_8_1(2, ref v);
            Block_8_1(3, ref v);
            Block_8_1(3, ref v);
            Avx.Store(data, v);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public unsafe void Sort8(int* data, int c) {
            throw new NotImplementedException("Sort8");
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public unsafe void Sort4(int* data) {
            var v = Avx2.MaskLoad(data, AlternatingMaskLo128);
            Block_4_2(2, ref v);
            Block_4_2(2, ref v);
            Avx2.MaskStore(data, AlternatingMaskLo128, v);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public unsafe void Sort4(int* data, int c) {
            throw new NotImplementedException("Sort4");
        }
    }
}
