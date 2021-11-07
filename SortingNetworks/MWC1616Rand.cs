using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace SortingNetworks
{
    /// <summary>
    /// Random number generator using MWC1616 (multiply with carry) algorithm.
    /// Code adapted from http://www.digicortex.net/node/22
    /// </summary>
    public struct MWC1616Rand : IUnsafeRandom
    {
        // Single array so that we can pin it only once.

        Vector128<uint> mask, m1, m2;   // Constants
        Vector128<uint> a, b;           // State

        public unsafe void Initialize(int[] seed) {
            if (seed.Length != 8)
                throw new ArgumentOutOfRangeException(nameof(seed), "The seed array must contain exactly 8 elements.");


            // Initialize MWC1616 masks and multipliers. Default values of 18000 and 30903 used for multipliers.
            mask = Vector128.Create(0xFFFFu);
            m1 = Vector128.Create(0x4650u);
            m2 = Vector128.Create(0x78B7u);
            fixed (int* p = seed) {
                a = Sse2.LoadVector128(p).AsUInt32();
                b = Sse2.LoadVector128(p + 4).AsUInt32();
            }
        }

        public unsafe void Get4(int* data) {
            var amask = Sse2.And(a, mask);
            var ashift = Sse2.ShiftRightLogical(a, 0x10);
            var amul = Sse41.MultiplyLow(amask, m1);
            a = Sse2.Add(amul, ashift);

            var bmask = Sse2.And(b, mask);
            var bshift = Sse2.ShiftRightLogical(b, 0x10);
            var bmul = Sse41.MultiplyLow(bmask, m2);
            b = Sse2.Add(bmul, bshift);

            var t1 = Sse2.And(b, mask);
            var t2 = Sse2.ShiftLeftLogical(a, 0x10);
            var r = Sse2.Add(t1, t2);
            Sse2.Store(data, r.AsInt32());
        }
    }
}
