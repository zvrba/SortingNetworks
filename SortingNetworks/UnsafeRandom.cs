using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace SortingNetworks
{
    /// <summary>
    /// Provides methods for fast, "unsafe" generation of integer or floating-point random numbers.
    /// </summary>
    public abstract class UnsafeRandom
    {
        readonly Vector128<int> oneMask;
        readonly Vector128<float> one;
        readonly Vector128<int> complement;

        protected UnsafeRandom() {
            oneMask = Vector128.Create(0x3F800000);
            one = Vector128.Create(1.0f);
            complement = Vector128.Create(-1);
        }

        /// <summary>
        /// Returns 4 random numbers in a vector.
        /// </summary>
        public abstract Vector128<int> Get4();

        /// <summary>
        /// Overwrites the initial 4 elements of <paramref name="data"/> with random 32-bit integers.
        /// </summary>
        /// <param name="data">
        /// Pointer to a memory chunk of at least 4 integers.  Behaviour is UNDEFINED if the allocated
        /// space for the chunk is shorter.
        /// </param>
        public unsafe void Get4(int* data) {
            var v = Get4();
            Sse2.Store(data, v);
        }

        /// <summary>
        /// Overwrites initial <paramref name="c"/> elements of <paramref name="data"/> with random 32-bit integers.
        /// </summary>
        /// <param name="data">
        /// Pointer to a memory chunk of at least <paramref name="c"/> integers.  Behaviour is UNDEFINED if the allocated
        /// space for the chunk is shorter.
        /// </param>
        /// <param name="c">
        /// Number of elements to write. Must be between 0 and 4.
        /// </param>
        public unsafe void Get(int* data, int c) {
            var v = Get4();
            var m = Sse2.ShiftRightLogical128BitLane(complement, (byte)(4 * (4 - c)));
            Avx2.MaskStore(data, m, v);
        }

        /// <summary>
        /// Overwrites the initial 4 elements of <paramref name="data"/> with floats in range <c>[-2^31, 2^31)</c>.
        /// </summary>
        /// <param name="data">
        /// Pointer to a memory chunk of at least 4 floats.
        /// Behaviour is UNDEFINED if the allocated space for the chunk is shorter.
        /// </param>
        public unsafe void Get4U(float* data) {
            var v = Get4();
            var f = Sse2.ConvertToVector128Single(v);
            Sse.Store(data, f);
        }

        /// <summary>
        /// Overwrites the initial 4 elements of <paramref name="data"/> with floats in range <c>[-2^31, 2^31)</c>.
        /// </summary>
        /// <param name="data">
        /// Pointer to a memory chunk of at least <paramref name="c"/> floats. Behaviour is UNDEFINED if the
        /// allocated space for the chunk is shorter.
        /// </param>
        /// <param name="c">
        /// Number of elements to write. Must be between 0 and 4.
        /// </param>
        public unsafe void Get4U(float* data, int c) {
            var v = Get4();
            var m = Sse2.ShiftRightLogical128BitLane(complement, (byte)(4 * (4 - c)));
            var f = Sse2.ConvertToVector128Single(v);
            Avx.MaskStore(data, m.AsSingle(), f);
        }

        /// <summary>
        /// Overwrites the initial 4 elements of <paramref name="data"/> with floats in range <c>[0, 1)</c>.
        /// </summary>
        /// <param name="data">
        /// Pointer to a memory chunk of at least 4 floats.
        /// Behaviour is UNDEFINED if the allocated space for the chunk is shorter.
        /// </param>
        public unsafe void Get4N(float* data) {
            var v = Get4();
            // Keep 23 MSB bits of the random integer and convert to [1.0,2.0)
            v = Sse2.Or(Sse2.ShiftRightLogical(v, 9), oneMask);
            var f = Sse.Subtract(v.AsSingle(), one);
            Sse.Store(data, f);
        }

        /// <summary>
        /// Overwrites the initial 4 elements of <paramref name="data"/> with floats in range <c>[0, 1)</c>.
        /// </summary>
        /// <param name="data">
        /// Pointer to a memory chunk of at least <paramref name="c"/> floats. Behaviour is UNDEFINED if the
        /// allocated space for the chunk is shorter.
        /// </param>
        /// <param name="c">
        /// Number of elements to write. Must be between 0 and 4.
        /// </param>
        public unsafe void Get4N(float* data, int c) {
            var v = Get4();
            v = Sse2.Or(Sse2.ShiftRightLogical(v, 9), oneMask);
            var m = Sse2.ShiftRightLogical128BitLane(complement, (byte)(4 * (4 - c)));
            var f = Sse.Subtract(v.AsSingle(), one);
            Avx.MaskStore(data, m.AsSingle(), f);
        }
    }
}
