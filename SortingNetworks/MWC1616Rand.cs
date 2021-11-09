using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace SortingNetworks
{
    /// <summary>
    /// Random number generator using MWC1616 (multiply with carry) algorithm.
    /// Code adapted from http://www.digicortex.net/node/22
    /// </summary>
    public sealed class MWC1616Rand : UnsafeRandom
    {
        // Single array so that we can pin it only once.

        Vector128<uint> mask, m1, m2;   // Constants
        Vector128<uint> a, b;           // State

        public MWC1616Rand(int[] seed) {
            if (seed.Length != 8)
                throw new ArgumentException("The seed array must contain exactly 8 elements.", nameof(seed));

            mask = Vector128.Create(0xFFFFu);
            m1 = Vector128.Create(0x4650u);
            m2 = Vector128.Create(0x78B7u);
            a = Vector128.Create((uint)seed[0], (uint)seed[1], (uint)seed[2], (uint)seed[3]);
            b = Vector128.Create((uint)seed[4], (uint)seed[5], (uint)seed[6], (uint)seed[7]);
        }

        public override Vector128<int> Get4() {
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
            return r.AsInt32();
        }
    }
}
