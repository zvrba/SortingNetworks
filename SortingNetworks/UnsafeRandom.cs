﻿using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace SortingNetworks
{
    public abstract class UnsafeRandom
    {
        readonly Vector128<int> oneMask;
        readonly Vector128<float> one;

        protected UnsafeRandom() {
            oneMask = Vector128.Create(0x3F800000);
            one = Vector128.Create(1.0f);
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
    }
}
